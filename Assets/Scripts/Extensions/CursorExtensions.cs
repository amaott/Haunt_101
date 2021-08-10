using UnityEngine;
using UnityEditor;
using System.Linq;

static public class CursorExtensions
{
	static Vector2 graspCursorIconOffset;
	static readonly float cursorIconScale = 0.3f;

	public static readonly Texture2D[] cursorGraspTextures = new[]
	{
		(Texture2D)Resources.Load("Cursor/Grasp/MotionGrasp1"),
		(Texture2D)Resources.Load("Cursor/Grasp/MotionGrasp2"),
		(Texture2D)Resources.Load("Cursor/Grasp/MotionGrasp3"),
		(Texture2D)Resources.Load("Cursor/Grasp/MotionGrasp4")
	};
	
	static public void CycleThroughGrasp()
	{
		Texture2D graspTexture = cursorGraspTextures[textureCounter];

		int iconWidth = Mathf.RoundToInt(graspTexture.width * cursorIconScale);
		int iconHeight = Mathf.RoundToInt(graspTexture.height * cursorIconScale);

		graspTexture = Resize(graspTexture, iconWidth, iconHeight);
		graspCursorIconOffset.Set(iconWidth / 2, iconHeight / 2);

		Cursor.SetCursor(graspTexture, graspCursorIconOffset, CursorMode.ForceSoftware);
	}

	static Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
	{
		RenderTexture rt = new RenderTexture(targetX, targetY, 24);
		RenderTexture.active = rt;
		Graphics.Blit(texture2D, rt);
		Texture2D result = new Texture2D(targetX, targetY, texture2D.format, mipChain: false);
		result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
		result.Apply();
		return result;
	}

	#region int textureCounter
	static int _textureCounter = 0;
	static int textureCounter
	{
		get
		{
			if (_textureCounter > cursorGraspTextures.Length - 1)
				_textureCounter = 0;

			return _textureCounter++;
		}
	}
	#endregion
}