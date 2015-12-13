using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Linq;
using System;

public class GameManager : MonoBehaviour {
	int counter = 0;

	private List<CodeNode> nodes;
	private UnityEngine.Object nodePrefab;

//	private CodeNode templateNode;

	void addNode() {
		var nodePrefabInst = Instantiate(nodePrefab) as GameObject;
		var node = nodePrefabInst.GetComponent<CodeNode>();
		nodes.Add(node);
	}

	void layoutNodes() {
		var elements = //new[]
			//			{node, node2}
			nodes
			.Select(x => new Treemap.Element<CodeNode> { Object = x, Value = 1})
			.ToList();

		var slice = Treemap.GetSlice(elements, 1, 0.35f); // TODO: minSliceRatio
		var rectangles = Treemap.GetRectangles(slice, 400, 400).ToList();

		foreach (var r in rectangles) {
			var node_inner = r.Slice.Elements.First().Object;

			var scalePos = 0.02f;
			var scalePadding = 0.9f;

			var fullWidth = r.Width * scalePos;
			var fullHeight = r.Height * scalePos;

			var x = r.X * scalePos + fullWidth / 2;
			var z = r.Y * scalePos + fullHeight / 2;

			node_inner.transform.position = new Vector3(x, 0.5f, z);
			node_inner.transform.localScale = new Vector3(
				fullWidth * scalePadding, 
				1, 
				fullHeight * scalePadding);
		}
	}

	// Use this for initialization
	void Start () {
		nodes = new List<CodeNode>();

		nodePrefab = Resources.Load("CodeNode");

		for (int i=0; i<4; ++i) {
			addNode();
		}

//		nodePrefabInst = Instantiate(nodePrefab) as GameObject;
//		node = nodePrefabInst.GetComponent<CodeNode>();
//		nodes.Add(node);
		layoutNodes();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("f")) {
			addNode();
			layoutNodes();
		}
	
	}
}
