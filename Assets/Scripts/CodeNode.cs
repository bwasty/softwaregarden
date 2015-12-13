using UnityEngine;
using System.Collections;

public class CodeNode : MonoBehaviour {

	public float startHeight = .25f;
	public float maxHeight = 4;

	public float thresholdGreen = 0.5f;
	public float thresholdYellow = 1;
	public float thresholdRed = 2;

	public Color colorBlue = Color.blue;
	public Color colorGreen = new Color32(17, 189, 61, 255);
	public Color colorYellow = Color.yellow;
	public Color colorRed = Color.red;
	public Color colorDead = Color.black;

	public float growthRate = 0.1f; // per second

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
		if (height < thresholdGreen) 
			mat.color = colorBlue;
		else if (height < thresholdYellow)
			mat.color = colorGreen;
		else if (height < thresholdRed)
			mat.color = colorYellow;
		else if (height < maxHeight)
			mat.color = colorRed;
		else if (height >= maxHeight) 
			mat.color = colorDead;
	}

	// Use this for initialization
	void Start () {
		Height = startHeight;
	}
	
	// Update is called once per frame
	void Update () {
//		if (Time.realtimeSinceStartup > 2) {
//			Height = 1;
//		}
//		if (Time.realtimeSinceStartup > 4) {
//			Height = 2;
//		}
//
//		if (Time.realtimeSinceStartup > 6) {
//			Height = 10;
//		}
			
		Height += growthRate * Time.deltaTime;
	}
}
