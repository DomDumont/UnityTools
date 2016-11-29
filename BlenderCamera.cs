#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

[InitializeOnLoad]
class BlenderCamera
	{

	static DrawCameraMode oldCameraMode;
	static bool lastCameraModeSaved;
	static bool focusModeOn;
	static Vector3 cameraPositionBeforeFocus;
	static Quaternion cameraRotationBeforeFocus;


	static Dictionary<GameObject,bool> dicObjectsVisibility = new Dictionary<GameObject, bool> ();
	static Dictionary<GameObject,bool> dicObjectsFocus = new Dictionary<GameObject, bool> ();

	static BlenderCamera ()
		{
		EditorApplication.update += Update;
		SceneView.onSceneGUIDelegate += OnScene;

		lastCameraModeSaved = false;
		focusModeOn = false;
		}

		
	private static void OnScene (SceneView sceneview)
		{
		
		UnityEditor.SceneView sceneView;
		Vector3 eulerAngles;
		Event current;
		Quaternion rotHelper;

		current = Event.current;

		if (!current.isKey || current.type != EventType.keyDown)
			{
			return;
			}

		sceneView = UnityEditor.SceneView.lastActiveSceneView;

		eulerAngles = sceneView.camera.transform.rotation.eulerAngles;
		rotHelper = sceneView.camera.transform.rotation;


		switch (current.keyCode)
			{

			case KeyCode.Keypad1:			
				if (current.control == false)
					{
					sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, Quaternion.Euler (new Vector3 (0f, 360f, 0f)));
					}
				else
					{
					sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, Quaternion.Euler (new Vector3 (0f, 180f, 0f)));
					}
				break;

			case KeyCode.Keypad2:
				sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, rotHelper * Quaternion.Euler (new Vector3 (-15f, 0f, 0f)));
				break;

			case KeyCode.Keypad3:
				if (current.control == false)
					sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, Quaternion.Euler (new Vector3 (0f, 270f, 0f)));
				else
					sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, Quaternion.Euler (new Vector3 (0f, 90f, 0f)));
				break;

			case KeyCode.Keypad4:
				sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, Quaternion.Euler (new Vector3 (eulerAngles.x, eulerAngles.y + 15f, eulerAngles.z)));
				break;

			case KeyCode.Keypad5:
				sceneView.orthographic = !sceneView.orthographic;
				break;

			case KeyCode.Keypad6:
				sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, Quaternion.Euler (new Vector3 (eulerAngles.x, eulerAngles.y - 15f, eulerAngles.z)));
				break;

			case KeyCode.Keypad7:
				if (current.control == false)
					sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, Quaternion.Euler (new Vector3 (90f, 0f, 0f)));
				else
					sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, Quaternion.Euler (new Vector3 (270f, 0f, 0f)));
				break;
			case KeyCode.Keypad8:
				sceneView.LookAtDirect (SceneView.lastActiveSceneView.pivot, rotHelper * Quaternion.Euler (new Vector3 (15f, 0f, 0f)));
				break;
			case KeyCode.KeypadPeriod:
				if (Selection.transforms.Length == 1)
					{
					sceneView.LookAtDirect (Selection.activeTransform.position, sceneView.camera.transform.rotation);
					}
				else
				if (Selection.transforms.Length > 1)
					{
					Vector3 tempVec = new Vector3 ();
					for (int i = 0; i < Selection.transforms.Length; i++)
						{
						tempVec += Selection.transforms [i].position;
						}
					sceneView.LookAtDirect ((tempVec / Selection.transforms.Length), sceneView.camera.transform.rotation);
					}
				break;
			case KeyCode.KeypadMinus:
				SceneView.RepaintAll ();
				sceneView.size *= 1.1f;
				break;
			case KeyCode.KeypadPlus:
				SceneView.RepaintAll ();
				sceneView.size /= 1.1f;
				break;

			case KeyCode.Z:
				if (lastCameraModeSaved == false)
					{
					oldCameraMode = sceneView.renderMode;
					lastCameraModeSaved = true;
					sceneview.renderMode = DrawCameraMode.Wireframe;
					}
				else
					{
					sceneview.renderMode = oldCameraMode;
					lastCameraModeSaved = false;
					}
				break;

			case KeyCode.H:
				ToggleVisibility ();
				if (current.control)
					{
					ClearVisibility ();
					}
				break;
			case KeyCode.Slash:
			//case KeyCode.KeypadDivide:
				KeepFocus ();
				break;
			}

		}

	static void KeepFocus ()
		{
		if (focusModeOn == false)
			{
			Debug.Log ("FocusModeOn == false");
			// This one is a little bit dangerous
			dicObjectsFocus.Clear ();

			GameObject[] gameObjects = Selection.gameObjects;
			Renderer[] renderers = UnityEngine.Object.FindObjectsOfType<Renderer> ();
			foreach (Renderer r in renderers)
				{
				
				if (ArrayUtility.Contains (gameObjects, r.gameObject) == false)
					{
					dicObjectsFocus [r.gameObject] = r.enabled;
					r.enabled = false; // like disable it for example. 
					}
				}
			//var view = SceneView.currentDrawingSceneView;
			//view.AlignViewToObject(Selection.activeGameObject.transform);
			SceneView.lastActiveSceneView.FrameSelected ();
			cameraPositionBeforeFocus = SceneView.currentDrawingSceneView.camera.transform.position;
			cameraRotationBeforeFocus = SceneView.currentDrawingSceneView.camera.transform.rotation;
			focusModeOn = true;
			}
		else
			{
			Debug.Log ("FocusModeOn == true");
			foreach (KeyValuePair<GameObject,bool> entry in dicObjectsFocus)
				{
				Renderer tempRenderer = entry.Key.GetComponent<Renderer> () as Renderer;

				if (tempRenderer)
					{
					tempRenderer.enabled = entry.Value;
					}
				else
					{
					Debug.Log ("Impossible to restore Renderer state for object " + entry.Key.name);
					}
				}
			dicObjectsFocus.Clear ();
			SceneView.currentDrawingSceneView.pivot = cameraPositionBeforeFocus;
			SceneView.currentDrawingSceneView.rotation = cameraRotationBeforeFocus;
			focusModeOn = false;
			}

		}

	static void ClearVisibility ()
		{
		Debug.Log ("ClearVisibility");
		foreach (KeyValuePair<GameObject,bool> entry in dicObjectsVisibility)
			{
			entry.Key.GetComponent<Renderer> ().enabled = entry.Value;
			}
		dicObjectsVisibility.Clear ();
		}

	static void ToggleVisibility ()
		{
		Debug.Log ("ToggleVisibility");
		Object[] objects = Selection.objects;
		foreach (Object tempObj in objects)
			{
			Debug.Log (tempObj.name);
			GameObject tempGO = tempObj as GameObject;
			if (tempGO != null)
				{
				Renderer[] renderers = tempGO.GetComponentsInChildren<Renderer> ();
				foreach (var r in renderers)
					{
					if (dicObjectsVisibility.ContainsKey (r.gameObject))
						{
						r.enabled = dicObjectsVisibility [r.gameObject];
						dicObjectsVisibility.Remove (r.gameObject);
						}
					else
						{
						dicObjectsVisibility [r.gameObject] = r.enabled;
						// Do something with the renderer here...
						r.enabled = false; // like disable it for example. 
						}
					}
					
				}
			}
		
		}

	public void OnDestroy ()
		{
		SceneView.onSceneGUIDelegate -= OnScene;
		}

	static void Update ()
		{

		}



	}

#endif