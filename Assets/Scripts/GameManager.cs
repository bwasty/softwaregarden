using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Linq;
using System;

public class GameManager : MonoBehaviour {

	private List<CodeNode> nodes;
	private UnityEngine.Object nodePrefab;

	private int locScore;

	public Text statsText;

	// Use this for initialization
	void Start () {
		nodes = new List<CodeNode>();
		nodePrefab = Resources.Load("CodeNode");

		for (int i=0; i<4; ++i) {
			addNode();
		}
		layoutNodes();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("f")) {
			addNode();
			layoutNodes();
		}

		updateScore();
	}

	void addNode() {
		var nodePrefabInst = Instantiate(nodePrefab) as GameObject;
		var node = nodePrefabInst.GetComponent<CodeNode>();
//		nodes.Add(node);
		nodes.Insert(nodes.Count/2, node);
	}

	void layoutNodes() {
		const int mapSize = 150;
		const float scalePos = 0.1f;
		const float scalePadding = 0.85f;

		// HACK: removing or reversing the ordering causes Unity to crash (infinite recursion in treemap layout?),
		// so to get a defined order we add i/1000 to the default weight of 1.
		var elements = nodes
			.Select((x, i) => new Treemap.Element<CodeNode> { Object = x, Value = 1+i/1000})
			.OrderByDescending (x => x.Value)
			.ToList();

		var slice = Treemap.GetSlice(elements, 1, 0.35f); // TODO: minSliceRatio
		var rectangles = Treemap.GetRectangles(slice, mapSize, mapSize).ToList();

		// by default the top right corner is at the origin, but we want the middle there
		var centerOffset = new Vector3(-mapSize*scalePos/2, 0, -mapSize*scalePos/2);

		foreach (var r in rectangles) {
			var node = r.Slice.Elements.First().Object;

			var fullWidth = r.Width * scalePos;
			var fullHeight = r.Height * scalePos;

			var x = r.X * scalePos + fullWidth / 2;
			var z = r.Y * scalePos + fullHeight / 2;

			node.transform.position = new Vector3(x, 0.5f, z) + centerOffset;
			node.transform.localScale = new Vector3(fullWidth * scalePadding, 1, fullHeight * scalePadding);
		}
	}

	void updateScore() {
		const int locPerHeight = 10;

		locScore = 0;
		var locRate = 0;
		foreach (var node in nodes) {
			locScore += (int)(node.Height * locPerHeight);
			locRate += (int)(node.growthRate * locPerHeight);
		}

		statsText.text = string.Format("LOC: {0}\nLOC/sec: {1}", locScore, locRate);
	}

}
