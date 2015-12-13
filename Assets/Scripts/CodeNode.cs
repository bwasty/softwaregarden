using UnityEngine;
using System.Collections;

public class CodeNode : MonoBehaviour {

	public static float startHeight = .25f;
	public static float maxHeight = 4;

	public static float thresholdYellow = 1;
	public static float thresholdRed = 2;

	private float height;
	public float Height {
		get { return height; }
		set {
			height = Mathf.Min(value, maxHeight);

			var t = gameObject.transform;

			var s = t.localScale;
			s.y = height;
			t.localScale = s;

			var p = t.position;
			p.y = height / 2;
			t.position = p;

			adaptColor();
		}
	}

	private void adaptColor() {
		var mat = GetComponent<Renderer>().material;
		if (height < thresholdYellow)
			mat.color = Color.green;
		else if (height < thresholdRed)
			mat.color = Color.yellow;
		else if (height < maxHeight)
			mat.color = Color.red;
		else if (height >= maxHeight) 
			mat.color = Color.black;
	}

	// Use this for initialization
	void Start () {
		Height = startHeight;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.realtimeSinceStartup > 3) {
			Height = 1;
		}
		if (Time.realtimeSinceStartup > 6) {
			Height = 2;
		}

		if (Time.realtimeSinceStartup > 10) {
			Height = 10;
		}
	}
}
