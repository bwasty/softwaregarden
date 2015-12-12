// Initially copied from http://pascallaurin42.blogspot.de/2013/12/implementing-treemap-in-c.html

using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class Treemap : MonoBehaviour {

	// Slice calculation

	public Slice<T> GetSlice<T>(IEnumerable<Element<T>> elements, float totalSize, float sliceWidth) {                 
		if (!elements.Any()) 
			return null;
		if (elements.Count() == 1) 
			return new Slice<T> { Elements = elements, Size = totalSize };

		var sliceResult = GetElementsForSlice(elements, sliceWidth);

		return new Slice<T> {
			Elements = elements,
			Size = totalSize,
			SubSlices = new[] { 
				GetSlice(sliceResult.Elements, sliceResult.ElementsSize, sliceWidth),
				GetSlice(sliceResult.RemainingElements, 1 - sliceResult.ElementsSize, sliceWidth)
			}
		};
	}

	private SliceResult<T> GetElementsForSlice<T>(IEnumerable<Element<T>> elements,	float sliceWidth) {
		var elementsInSlice = new List<Element<T>>();
		var remainingElements = new List<Element<T>>();
		float current = 0;
		float total = elements.Sum(x => x.Value);

		foreach (var element in elements) {
			if (current > sliceWidth) {
				remainingElements.Add(element);
			}
			else {
				elementsInSlice.Add(element);
				current += element.Value / total;
			}
		}

		return new SliceResult<T> { 
			Elements = elementsInSlice, 
			ElementsSize = current,
			RemainingElements = remainingElements
		};
	}

	public class SliceResult<T> {
		public IEnumerable<Element<T>> Elements { get; set; }
		public float ElementsSize { get; set; }
		public IEnumerable<Element<T>> RemainingElements { get; set; }
	}

	public class Slice<T> {
		public float Size { get; set; }
		public IEnumerable<Element<T>> Elements { get; set; }
		public IEnumerable<Slice<T>> SubSlices { get; set; }
	}

	public class Element<T> {
		public T Object { get; set; }
		public float Value { get; set; }
	}


	// Generating rectangles using leaf slice (slice with only one element in it)

	public IEnumerable<SliceRectangle<T>> GetRectangles<T>(Slice<T> slice, int width, int height) {
		var area = new SliceRectangle<T> { Slice = slice, Width = width, Height = height };

		foreach (var rect in GetRectangles(area)) {
			// Make sure no rectangle go outside the original area
			if (rect.X + rect.Width > area.Width) rect.Width = area.Width - rect.X;
			if (rect.Y + rect.Height > area.Height) rect.Height = area.Height - rect.Y;

			yield return rect;
		}
	}

	private IEnumerable<SliceRectangle<T>> GetRectangles<T>(SliceRectangle<T> sliceRectangle) {
		var isHorizontalSplit = sliceRectangle.Width >= sliceRectangle.Height;
		var currentPos = 0;
		foreach (var subSlice in sliceRectangle.Slice.SubSlices) {
			var subRect = new SliceRectangle<T> { Slice = subSlice };
			int rectSize;

			if (isHorizontalSplit) {
				rectSize = (int)Mathf.Round(sliceRectangle.Width * subSlice.Size);
				subRect.X = sliceRectangle.X + currentPos;
				subRect.Y = sliceRectangle.Y;
				subRect.Width = rectSize;
				subRect.Height = sliceRectangle.Height;
			}
			else {
				rectSize = (int)Mathf.Round(sliceRectangle.Height * subSlice.Size);
				subRect.X = sliceRectangle.X;
				subRect.Y = sliceRectangle.Y + currentPos;
				subRect.Width = sliceRectangle.Width;
				subRect.Height = rectSize;
			}

			currentPos += rectSize;

			if (subSlice.Elements.Count() > 1) {
				foreach (var sr in GetRectangles(subRect))
					yield return sr;
			}
			else if (subSlice.Elements.Count() == 1) {
				yield return subRect;
			}
		}
	}

	public class SliceRectangle<T> {
		public Slice<T> Slice { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
	}


	// Drawing the rectangles in Unity
	public void DrawTreemap<T>(IEnumerable<SliceRectangle<T>> rectangles/*, int width,  int height*/) {
		foreach (var r in rectangles) {
			var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			var scalePos = 0.02f;
			var scalePadding = 0.9f;

			var fullWidth = r.Width * scalePos;
			var fullHeight = r.Height * scalePos;

			var x = r.X * scalePos + fullWidth / 2;
			var z = r.Y * scalePos + fullHeight / 2;

			cube.transform.position = new Vector3(x, 0.5f, z);
			cube.transform.localScale = new Vector3(fullWidth * scalePadding, 1, fullHeight * scalePadding);
		}

	}

	void Start() {
		const int Width = 400;
		const int Height = 300;
		const float MinSliceRatio = 0.35f;

		var elements = new[] 
		    { 24, 45, 32, 87, 34, 58, 10, 4, 5, 9, 52, 34 }
//			{5, 5, 5, 5}
			.Select (x => new Element<string> { Object = x.ToString(), Value = x })
			.OrderByDescending (x => x.Value)
			.ToList();

		var slice = GetSlice(elements, 1, MinSliceRatio);

		var rectangles = GetRectangles(slice, Width, Height)
			.ToList();

		DrawTreemap(rectangles/*, Width, Height*/);
	}
}