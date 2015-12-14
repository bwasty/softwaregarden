using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//using System.Xml.Linq;
using System.Linq.Expressions;
using System.Linq;
using System;

public class GameManager : MonoBehaviour {

	public List<CodeNode> nodes;
	private UnityEngine.Object nodePrefab;

	private int locScore;

	public Text statsText;
	public Text gameOverText;
//	public int maxNodes = 512;
	public int maxDead = 8;

	public bool gameRunning = true;

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
//		if (Input.GetKeyDown("f")) {
//			addNode();
//			layoutNodes();
//		}

		updateScore();
	}

	public void addNode(CodeNode node=null, CodeNode after=null) {
		if (!node) {
			var nodePrefabInst = Instantiate(nodePrefab) as GameObject;
			node = nodePrefabInst.GetComponent<CodeNode>();
			node.manager = this;
		}

		if (after)
			nodes.Insert(nodes.IndexOf(after) + 1, node);
		else
			nodes.Add(node);
	}

	public void layoutNodes() {
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
		if (!gameRunning)
			return;

		const int locPerHeight = 10;

		locScore = 0;
		var locRate = 0;
		var numDead = 0;
		var locLost = 0;
		foreach (var node in nodes) {
			var loc = (int)(node.Height * locPerHeight);

			if (node.Height == node.maxHeight) {
				++numDead;
				locLost += loc;
			} else {
				locScore += loc;
				locRate += (int)(node.growthRate * locPerHeight);
			}
			 
		}

		if (numDead > maxDead) {
			gameRunning = false;
//			gameOverText.text += string.Format("\nFinal Score: {0} Lines of Code");
			gameOverText.gameObject.SetActive(true);
		}

		statsText.text = string.Format("LOC: {0}\nLOC/sec: {1}\nLOC lost: {2}", locScore, locRate, locLost);
	}

}
