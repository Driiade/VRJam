using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VR;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
public class ReprojectionCompatibilityCheck
{
	static ReprojectionCompatibilityCheck()
	{
		EditorApplication.update += Update;
	}

	static void Update()
	{
#if UNITY_5_6_OR_NEWER
		if (PlayerSettings.stereoRenderingPath != StereoRenderingPath.MultiPass)
			Debug.LogError("The Reprojection Demo is ONLY supported under MultiPass stereo rendering path ");

		BuildTargetGroup target = EditorUserBuildSettings.selectedBuildTargetGroup;
		for (int tier = (int)UnityEngine.Rendering.GraphicsTier.Tier1; tier <= (int)UnityEngine.Rendering.GraphicsTier.Tier3; tier++)
		{
			var tierSettings = UnityEditor.Rendering.EditorGraphicsSettings.GetTierSettings(target, (UnityEngine.Rendering.GraphicsTier)tier);
			if (tierSettings.renderingPath != RenderingPath.Forward)
				Debug.LogError("The Reprojection Demo is implemented under forward renderer only");
		}
#else
		Debug.LogError("The Reprojection Demo is implemented under Unity 5.6 or newer versions ");
#endif
	}
}
#endif


[RequireComponent(typeof(Camera))]
public class GlobalReprojectionManager : MonoBehaviour
{
	private Camera ownerCam;
	private RenderTexture renderTex = null;
	private RenderTexture depthTex;

	private RenderTextureFormat format;
	private int depthBits = 24;

	public bool allowReprojection;
	public bool showReprojectionMask;

	private Material copyDepthMaterial;
	private Material reprojectionPassMaterial;
	private Material depthRestoreMaterial;

	// Use this for initialization
	void OnEnable()
	{
		// By default, Unity 5.6 is using ARGBHalf
		format = RenderTextureFormat.ARGBHalf;

		// Make sure the following shaders inside your resources folder or being referenced
		if (copyDepthMaterial == null)
			copyDepthMaterial = new Material(Shader.Find("Custom/Copy Depth"));

		if (reprojectionPassMaterial == null)
			reprojectionPassMaterial = new Material(Shader.Find("Custom/GlobalReprojectionPass"));

		if (depthRestoreMaterial == null)
			depthRestoreMaterial = new Material(Shader.Find("Custom/Depth Restore"));

		ownerCam = (Camera)GetComponent<Camera>();
		ownerCam.depthTextureMode = DepthTextureMode.Depth;
		CreateRenderTex();
		BuildCommandBuffers(ownerCam);
	}

	// Get matrix to transform from tracking space to world space 
	// Given the facts we know both head camera's world transform information and tracking space information
	static Matrix4x4 TrackingSpaceToWorldSpace(Camera headCamera)
	{
		Vector3 headPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
		Quaternion headOrientation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head);
		Matrix4x4 headPoseInTrackingSpace = Matrix4x4.TRS(headPosition, headOrientation, Vector3.one);

		// Transform from tracking-Space to head-Space
		Matrix4x4 trackingSpaceToHeadSpace = headPoseInTrackingSpace.inverse;

		// Transform from head space to world space
		Matrix4x4 ret = headCamera.transform.localToWorldMatrix * trackingSpaceToHeadSpace;
		return ret;
	}

	// Get specified eye's world to camera matrix
	Matrix4x4 GetVRWorldToCameraMatrix(Camera cam, UnityEngine.XR.XRNode eye)
	{
		Vector3 pos = UnityEngine.XR.InputTracking.GetLocalPosition(eye);
		Quaternion rot = UnityEngine.XR.InputTracking.GetLocalRotation(eye);
		Matrix4x4 m = Matrix4x4.TRS(pos, rot, Vector3.one);
		m = TrackingSpaceToWorldSpace(cam) * m;
		m = Matrix4x4.Inverse(m);

		// Unity uses OpenGL convention, forward is -Z
		// http://docs.unity3d.com/ScriptReference/Camera-worldToCameraMatrix.html
		// so negate z row

		m.m20 *= -1f;
		m.m21 *= -1f;
		m.m22 *= -1f;
		m.m23 *= -1f;

		return m;
	}


	// Get specified eye's world to view projection matrix
	Matrix4x4 CalcViewProjection(Camera cam, UnityEngine.XR.XRNode eye)
	{
		Matrix4x4 viewMat = GetVRWorldToCameraMatrix(cam, eye);
		Matrix4x4 projMat = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
		return projMat * viewMat;
	}


	void CreateRenderTex()
	{
		int width = (int)GetComponent<Camera>().pixelWidth;
		int height = (int)GetComponent<Camera>().pixelHeight;
		if (renderTex == null)
		{
			renderTex = new RenderTexture(width, height, depthBits, format);
			renderTex.Create();
			Debug.Log("Render texture size: " + renderTex.width + ", " + renderTex.height);
			renderTex.hideFlags = HideFlags.DontSave;
		}

		if (depthTex == null)
		{
			// Need this because shader can't read from Unity RenderBuffer:
			depthTex = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
			depthTex.filterMode = FilterMode.Point;
			depthTex.Create();
			depthTex.hideFlags = HideFlags.DontSave;
		}
	}

	// Create command buffer for frame buffer copying
	private CommandBuffer bufferSavingCommands;
	private CommandBuffer depthRestoreCommands;
	private CommandBuffer reprojectionCommands;

	private void BuildCommandBuffers(Camera targetCamera)
	{
		if (bufferSavingCommands == null)
		{
			bufferSavingCommands = new CommandBuffer();
			bufferSavingCommands.name = "Blit Left eye depth and color";
			bufferSavingCommands.Blit(BuiltinRenderTextureType.CurrentActive, renderTex);
			bufferSavingCommands.Blit(BuiltinRenderTextureType.Depth, depthTex, copyDepthMaterial);
		}

		if (reprojectionCommands == null)
		{
			reprojectionCommands = new CommandBuffer();
			reprojectionCommands.name = "Reprojection Global Pass";
			reprojectionCommands.SetGlobalTexture("_OtherEyeTex", renderTex);
			reprojectionCommands.SetGlobalTexture("_OtherEyeDepthTexture", depthTex);
			reprojectionCommands.Blit(BuiltinRenderTextureType.None, BuiltinRenderTextureType.CameraTarget, reprojectionPassMaterial);
		}

		if (depthRestoreCommands == null)
		{
			depthRestoreCommands = new CommandBuffer();
			depthRestoreCommands.name = "Depth Restore Pass";
			depthRestoreCommands.Blit(BuiltinRenderTextureType.None, BuiltinRenderTextureType.CameraTarget, depthRestoreMaterial);
		}
	}

	void OnPreRender()
	{
		if (ownerCam && allowReprojection)
		{
			if (ownerCam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
			{
				reprojectionCommands.SetGlobalMatrix("_OtherEyeViewProj", CalcViewProjection(ownerCam, UnityEngine.XR.XRNode.LeftEye));
				reprojectionCommands.SetGlobalMatrix("_InvViewProj", CalcViewProjection(ownerCam, UnityEngine.XR.XRNode.RightEye).inverse);
				ownerCam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, reprojectionCommands);
				ownerCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, depthRestoreCommands);
			}
			else if (ownerCam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
			{
				ownerCam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, bufferSavingCommands);
			}
		}
	}

	void OnPostRender()
	{
		ownerCam.RemoveAllCommandBuffers();
	}
}

