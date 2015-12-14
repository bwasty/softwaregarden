using UnityEngine;
using System.Collections;

public class CodeNode : MonoBehaviour {
	public GameManager manager;

	public float startHeight = .15f;
	public float maxHeight = 4;

	public float thresholdGreen = 0.5f;
	public float thresholdYellow = 1.5f;
	public float thresholdOrange = 2.5f;
	public float thresholdRed = 3.75f;

	public Color colorBlue = Color.blue;
	public Color colorGreen = new Color32(17, 189, 61, 255);
	public Color colorYellow = Color.yellow;
	public Color colorOrange = new Color32(244, 178, 92, 155);
	public Color colorRed = Color.red;
	public Color colorDead = Color.black;

	public float growthRate = 0.1f; // per second

//	public float startArea = 100;
//	public float Area {get; set;}

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

	// Use this for initialization
	void Start () {
		Height = startHeight;
//		Area = startArea;
	}

	private void adaptColor() {
		var mat = GetComponent<Renderer>().material;
		if (height < thresholdGreen) 
			mat.color = colorBlue;
		else if (height < thresholdYellow)
			mat.color = colorGreen;
		else if (height < thresholdOrange)
			mat.color = colorYellow;
		else if (height < thresholdRed)
			mat.color = colorOrange;
		else if (height < maxHeight)
			mat.color = colorRed;
		else if (height >= maxHeight) 
			mat.color = colorDead;
	}

	void splitNode() {
		var clone = Instantiate(gameObject);
		var cloneNode = clone.GetComponent<CodeNode>();

		var heightSplitPoint = Random.Range(.2f, .7f);

		Height *= heightSplitPoint;
		cloneNode.Height *= 1f - heightSplitPoint;

		manager.addNode(cloneNode, this);
		manager.layoutNodes();
	}

	void OnMouseDown() {
		if (!manager.gameRunning || manager.nodes.Count > manager.maxNodes)
			return;

		if (height > thresholdYellow && height < maxHeight)
			splitNode();
	}
	
	// Update is called once per frame
	void Update () {
		if (manager.gameRunning)
			Height += growthRate * Time.deltaTime;
	}
}
