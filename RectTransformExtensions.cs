using UnityEngine;
using UnityEditor;

public class RectTransformExtensions
{
	/// <summary>
	/// Context menu option from inspector
	/// </summary>
	/// <param name="command"></param>
	[MenuItem("CONTEXT/RectTransform/Make Resolution Flexible")]
	private static void MakeResolutionFlexible_Inspector(MenuCommand command)
	{
		RectTransform rectTransform = (RectTransform)command.context;
		if (rectTransform != null)
		{
			MakeResolutionFlexible_Transform(rectTransform);
		}
	}

	/// <summary>
	/// GameObject menu option and hierarchy option
	/// </summary>
	[MenuItem("GameObject/UI/RectTransform/Make Resolution Flexible %r", false, 100)]
	private static void MakeResolutionFlexible_Hierarchy()
	{
		GameObject target = Selection.activeGameObject;
		if (target != null)
		{
			RectTransform rectTransform = target.GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				MakeResolutionFlexible_Transform(rectTransform);
			}
		}
	}

	/// <summary>
	/// Given a rect transform, perform the operation
	/// </summary>
	/// <param name="rectTransform"></param>
	private static void MakeResolutionFlexible_Transform(RectTransform rectTransform)
	{
		if (rectTransform.childCount > 0 && EditorUtility.DisplayDialog("Rect Transform",
				"Apply flexible resolution to all children?", "Yes", "No, just this transform"))
		{
			MakeResolutionFlexible_Recursive(rectTransform);
		}
		else
		{
			MakeResolutionFlexible(rectTransform);
		}
	}

	/// <summary>
	/// Recursivelty set all children of a RectTransform to be Resolution flexible
	/// </summary>
	/// <param name="rectTransform"></param>
	private static void MakeResolutionFlexible_Recursive(RectTransform rectTransform)
	{
		//if we have children, set them up first
		if (rectTransform.childCount != 0)
		{
			foreach (Transform t in rectTransform.transform)
			{
				RectTransform rectT = t.GetComponent<RectTransform>();
				if (rectT != null)
				{
					MakeResolutionFlexible_Recursive(rectT);
				}
			}
		}
		//set this transform
		MakeResolutionFlexible(rectTransform);
	}

	/// <summary>
	/// Set up a RectTransform to be resolution flexible
	/// </summary>
	/// <param name="rectTransform"></param>
	private static void MakeResolutionFlexible(RectTransform rectTransform)
	{
		RectTransform parentTransform = rectTransform.parent.GetComponent<RectTransform>();
		if (parentTransform == null)
		{
			Debug.LogWarning("Base canvas cannot be re-anchored, doing nothing for this particular transform.\n");
		}
		else
		{
			//calculate the bounds of this rect and the parent
			Vector3[] rectCorners = new Vector3[4];
			rectTransform.GetWorldCorners(rectCorners);
			Vector3 worldMin = rectCorners[0];
			Vector3 worldMax = rectCorners[2];
			Vector3[] parentCorners = new Vector3[4];
			parentTransform.GetWorldCorners(parentCorners);
			Vector3 parentWorldMin = parentCorners[0];
			Vector3 parentWorldMax = parentCorners[2];
			float parentWidth = parentWorldMax.x - parentWorldMin.x;
			float parentHeight = parentWorldMax.y - parentWorldMin.y;
			//anchors expressed as a percentage within the parent rect
			Vector2 targetMin = Vector2.zero;
			Vector2 targetMax = Vector2.zero;
			targetMin.x = (worldMin.x - parentWorldMin.x) / parentWidth;
			targetMax.x = (worldMax.x - parentWorldMin.x) / parentWidth;
			targetMin.y = (worldMin.y - parentWorldMin.y) / parentHeight;
			targetMax.y = (worldMax.y - parentWorldMin.y) / parentHeight;
			//here's where we can undo things if we want
			Undo.RecordObject(rectTransform, "Make RectTransform Resolution Flexible");
			//store things...
			Vector2 storedSize = rectTransform.rect.size;
			Vector2 storedPosition = rectTransform.position;
			rectTransform.anchorMin = targetMin;
			rectTransform.anchorMax = targetMax;
			//need to reset the rect since moving the anchors in code resizes it (YAY UNITY)
			rectTransform.offsetMin += new Vector2((rectTransform.rect.size.x - storedSize.x),
				(rectTransform.rect.size.y - storedSize.y));
			rectTransform.offsetMax -= new Vector2((rectTransform.rect.size.x - storedSize.x),
				(rectTransform.rect.size.y - storedSize.y));
			rectTransform.position = storedPosition;
		}
	}
}
