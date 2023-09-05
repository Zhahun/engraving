using System;


struct Point
{
	public short x;
	public short y;
}

struct Rect
{
	public Point low;     // lower-left point
	public Point high;    // higher-right point
}

struct Leaf
{
	//Label L;
	//BoxBounded item;
};

struct Branch
{
	Rect mbb;
	Node child;
};

enum Entry: Leaf | Branch
{
	Leaf,
	Branch 
}

struct Node
{
	public int level;
	public Entry[] entries;
	short min_children;
	short max_children;
	//SplitStrategy split_strat;
}


public class RTree
{
	Node root;
	//Dictionary<RTreeKey, Arc> lookup_map;
	
	public RTree()
	{
	}
}
