/*
 * Adaptation of https://github.com/imranghory/treemap-squared/ for use in Unity3D. Original header:
 *
 * treemap-squarify.js - open source implementation of squarified treemaps
 *
 * Treemap Squared 0.5 - Treemap Charting library 
 *
 * https://github.com/imranghory/treemap-squared/
 *
 * Copyright (c) 2012 Imran Ghory (imranghory@gmail.com)
 * Licensed under the MIT (http://www.opensource.org/licenses/mit-license.php) license.
 *
 *
 * Implementation of the squarify treemap algorithm described in:
 * 
 * Bruls, Mark; Huizing, Kees; van Wijk, Jarke J. (2000), "Squarified treemaps" 
 * in de Leeuw, W.; van Liere, R., Data Visualization 2000: 
 * Proc. Joint Eurographics and IEEE TCVG Symp. on Visualization, Springer-Verlag, pp. 33–42.
 *
 * Paper is available online at: http://www.win.tue.nl/~vanwijk/stm.pdf
 *
 * The code in this file is completeley decoupled from the drawing code so it should be trivial
 * to port it to any other vector drawing library. Given an array of datapoints this library returns
 * an array of cartesian coordinates that represent the rectangles that make up the treemap.
 *
 * The library also supports multidimensional data (nested treemaps) and performs normalization on the data.
 * 
 * See the README file for more details.
 */

#pragma strict

// sumArray - sums a single dimensional array 
static function sumArray(arr : Array) {
    var sum = 0;
    var i : int;

    for (i = 0; i < arr.length; i++) {
    	var elem: float = arr[i];
        sum += elem;
    }
    return sum;
};


class Container {
	var xoffset : float;
	var yoffset : float;
	var height : float;
	var width : float;

	function Container(xoffset:float, yoffset:float, width:float, height:float) {
        this.xoffset = xoffset; // offset from the the top left hand corner
        this.yoffset = yoffset; // ditto 
        this.height = height;
        this.width = width;
	}

    function shortestEdge() {
        return Mathf.Min(this.height, this.width);
    };

    // getCoordinates - for a row of boxes which we've placed 
    //                  return an array of their cartesian coordinates
    function getCoordinates(row : Array) {
        var coordinates = Array();
        var subxoffset = this.xoffset;
        var subyoffset = this.yoffset; //our offset within the container
        var areawidth = TreemapSquarify.sumArray(row) / this.height;
        var areaheight = TreemapSquarify.sumArray(row) / this.width;
        var i : int;
        var elem: float;

        if (this.width >= this.height) {
            for (i = 0; i < row.length; i++) {
            	elem = row[i];
                coordinates.push([subxoffset, subyoffset, subxoffset + areawidth, subyoffset + elem / areawidth]);
                subyoffset = subyoffset + elem / areawidth;
            }
        } else {
            for (i = 0; i < row.length; i++) {
            	elem = row[i];
                coordinates.push([subxoffset, subyoffset, subxoffset + elem / areaheight, subyoffset + areaheight]);
                subxoffset = subxoffset + elem / areaheight;
            }
        }
        return coordinates;
    };

    // cutArea - once we've placed some boxes into an row we then need to identify the remaining area, 
    //           this function takes the area of the boxes we've placed and calculates the location and
    //           dimensions of the remaining space and returns a container box defined by the remaining area
    function cutArea(area : float) {
        var newcontainer;

        if (this.width >= this.height) {
            var areawidth = area / this.height;
            var newwidth = this.width - areawidth;
            newcontainer = new Container(this.xoffset + areawidth, this.yoffset, newwidth, this.height);
        } else {
            var areaheight = area / this.width;
            var newheight = this.height - areaheight;
            newcontainer = new Container(this.xoffset, this.yoffset + areaheight, this.width, newheight);
        }
        return newcontainer;
    };

}

class Treemap {

    // isArray - checks if arr is an array
    var isArray = function(arr) {
        return arr instanceof Array;
    };

    // sumMultidimensionalArray - sums the values in a nested array (aka [[0,1],[[2,3]]])
    function sumMultidimensionalArray(arr : Array) : float {
        var i : int;
        var total = 0;

        if(isArray(arr[0])) {
            for(i=0; i<arr.length; i++) {
                total += sumMultidimensionalArray(arr[i]);
            }
        } else {
            total = TreemapSquarify.sumArray(arr);
        }
        return total;
    };

    // improveRatio - implements the worse calculation and comparision as given in Bruls
    //                (note the error in the original paper; fixed here) 
    function improvesRatio(currentrow : Array, nextnode, length) {
        var newrow : Array; 

        if (currentrow.length == 0) {
            return true;
        }

        for (var i = 0; i < currentrow.length; ++i) {
        	newrow.Push(currentrow[i]);
        }
        //newrow = currentrow.slice();
        newrow.Push(nextnode);
        
        var currentratio = calculateRatio(currentrow, length);
        var newratio = calculateRatio(newrow, length);
        
        // the pseudocode in the Bruls paper has the direction of the comparison
        // wrong, this is the correct one.
        return currentratio >= newratio; 
    };

    // calculateRatio - calculates the maximum width to height ratio of the
    //                  boxes in this row
    var calculateRatio = function(row : Array, length) {
        var min = Mathf.Min(row.ToBuiltin(float) as float[]);
        var max = Mathf.Max(row.ToBuiltin(float) as float[]);
        var sum = TreemapSquarify.sumArray(row);
        return Mathf.Max(Mathf.Pow(length, 2) * max / Mathf.Pow(sum, 2), Mathf.Pow(sum, 2) / (Mathf.Pow(length, 2) * min));
    };

    // squarify  - as per the Bruls paper 
    //             plus coordinates stack and containers so we get 
    //             usable data out of it 
    function squarify(data : Array, currentrow : Array, container : Container, stack : Array) : Array {
        var length;
        var nextdatapoint;
        var newcontainer;

        if (data.length == 0) {
            stack.push(container.getCoordinates(currentrow));
            return;
        }

        length = container.shortestEdge();
        nextdatapoint = data[0];
        
        if (improvesRatio(currentrow, nextdatapoint, length)) {
            currentrow.push(nextdatapoint);
            squarify(data.slice(1), currentrow, container, stack);
        } else {
            newcontainer = container.cutArea(TreemapSquarify.sumArray(currentrow)/*, stack*/);
            stack.push(container.getCoordinates(currentrow));
            squarify(data, [], newcontainer, stack);
        }
        return stack;
    };

    // flattenTreemap - squarify implementation returns an array of arrays of coordinates
    //                  because we have a new array everytime we switch to building a new row
    //                  this converts it into an array of coordinates.
    var flattenTreemap = function(rawtreemap : Array) {
        var flattreemap = Array();
        var i:int;
        var j:int;

        for (i = 0; i < rawtreemap.length; i++) {
        	var elem:Array = rawtreemap[i];
            for (j = 0; j < elem.length; j++) {
                flattreemap.push(elem[j]);
            }
        }
        return flattreemap;
    };

    // treemapMultidimensional - takes multidimensional data (aka [[23,11],[11,32]] - nested array)
    //                           and recursively calls itself using treemapSingledimensional
    //                           to create a patchwork of treemaps and merge them
    function treemapMultidimensional(data : Array, width : float, height : float, xoffset : float, yoffset : float) : Array {
        xoffset = (typeof xoffset == "undefined") ? 0 : xoffset;
        yoffset = (typeof yoffset == "undefined") ? 0 : yoffset;
        
        var mergeddata = [];
        var mergedtreemap:Array;
        var results = Array();
        var i : int;

        if(isArray(data[0])) { // if we've got more dimensions of depth
            for(i=0; i<data.length; i++) {
                mergeddata[i] = sumMultidimensionalArray(data[i]);
            }
            mergedtreemap = treemapSingledimensional(mergeddata, width, height, xoffset, yoffset);
            
            for(i=0; i<data.length; i++) {
            	var merge_elem: Array = mergedtreemap[i];
            	var merge_elem_2: float[] = merge_elem.ToBuiltin(float) as float[];
                results.push(treemapMultidimensional(
                	data[i], 
                	merge_elem_2[2] - merge_elem_2[0], 
                	merge_elem_2[3] - merge_elem_2[1], 
                	merge_elem_2[0], 
                	merge_elem_2[1]
                ));
            }
        } else {
            results = treemapSingledimensional(data,width,height, xoffset, yoffset);
        };
        return results;
    };

    // treemapSingledimensional - simple wrapper around squarify
    var treemapSingledimensional = function(data, width:float, height:float, xoffset:float, yoffset:float) : Array {
        xoffset = (typeof xoffset == "undefined") ? 0 : xoffset;
        yoffset = (typeof yoffset == "undefined") ? 0 : yoffset;

        var rawtreemap = squarify(normalize(data, width * height), [], new Container(xoffset, yoffset, width, height), []);
        return flattenTreemap(rawtreemap);
    };

    // normalize - the Bruls algorithm assumes we're passing in areas that nicely fit into our 
    //             container box, this method takes our raw data and normalizes the data values into  
    //             area values so that this assumption is valid.
    var normalize = function (data : float[], area : float) {
        var normalizeddata = [];
        var sum = TreemapSquarify.sumArray(data);
        var multiplier = area / sum;
        var i : int;

        for (i = 0; i < data.length; i++) {
            normalizeddata[i] = data[i] * multiplier;
        }
        return normalizeddata;
    };

	function generate() {
        return treemapMultidimensional; 
    }

}