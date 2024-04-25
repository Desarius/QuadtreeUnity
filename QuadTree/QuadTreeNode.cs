using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNode<T> where T : MonoBehaviour
{
    private Bounds boundary;
    private List<T> objects;
    private int capacity;
    private bool divided;
    private QuadTreeNode<T> northWest;
    private QuadTreeNode<T> northEast;
    private QuadTreeNode<T> southWest;
    private QuadTreeNode<T> southEast;
    
    private static Stack<QuadTreeNode<T>> nodePool = new Stack<QuadTreeNode<T>>();

    public QuadTreeNode(Bounds boundary, int capacity)
    {
        this.boundary = boundary;
        this.capacity = capacity;
        this.objects = new List<T>();
        this.divided = false;
    }

    public bool Insert(T obj)
    {
        if (!boundary.Contains(obj.transform.position)) return false;

        if (objects.Count < capacity)
        {
            objects.Add(obj);
            return true;
        }

        if (!divided)
        {
            Subdivide();
        }

        return (northWest.Insert(obj) || northEast.Insert(obj) || southWest.Insert(obj) || southEast.Insert(obj));
    }

    private void Subdivide()
    {
        float x = boundary.center.x;
        float y = boundary.center.y;
        float w = boundary.size.x / 2;
        float h = boundary.size.y / 2;

        Bounds nw = new Bounds(new Vector3(x - w / 2, y + h / 2, 0), new Vector3(w, h, 0));
        northWest = nodePool.Count > 0 ? nodePool.Pop() : new QuadTreeNode<T>(nw, capacity);

        Bounds ne = new Bounds(new Vector3(x + w / 2, y + h / 2, 0), new Vector3(w, h, 0));
        northEast = nodePool.Count > 0 ? nodePool.Pop() : new QuadTreeNode<T>(ne, capacity);
   
        Bounds sw = new Bounds(new Vector3(x - w / 2, y - h / 2, 0), new Vector3(w, h, 0));
        southWest = nodePool.Count > 0 ? nodePool.Pop() : new QuadTreeNode<T>(sw, capacity);
        
        Bounds se = new Bounds(new Vector3(x + w / 2, y - h / 2, 0), new Vector3(w, h, 0));
        southEast = nodePool.Count > 0 ? nodePool.Pop() : new QuadTreeNode<T>(se, capacity);


        divided = true;
    }

    public List<T> QueryRange(Vector2 center, float radius, float maxObjectSize)
    {
        List<T> found = new List<T>();

        Bounds queryBounds = new Bounds(center, new Vector2(radius * 2, radius * 2));
        if (!boundary.Intersects(queryBounds))
        {
            return found;
        }

        // If the entire node is within the query range, add all objects without checking them individually
        if (boundary.size.x <= radius + maxObjectSize && boundary.size.y <= radius + maxObjectSize &&
            Vector2.Distance(new Vector2(boundary.center.x, boundary.center.y), center) <= radius)
        {
            found.AddRange(objects);
        }
        else
        {
            foreach (T obj in objects)
            {
                if (Vector2.Distance(obj.transform.position, center) <= radius)
                {
                    found.Add(obj);
                }
            }
        }

        if (divided)
        {
            found.AddRange(northWest.QueryRange(center, radius, maxObjectSize));
            found.AddRange(northEast.QueryRange(center, radius, maxObjectSize));
            found.AddRange(southWest.QueryRange(center, radius, maxObjectSize));
            found.AddRange(southEast.QueryRange(center, radius, maxObjectSize));
        }

        return found;
    }

    public int CountElements()
    {
        int count = objects.Count;

        if (divided)
        {
            count += northWest.CountElements();
            count += northEast.CountElements();
            count += southWest.CountElements();
            count += southEast.CountElements();
        }

        return count;
    }
    
    public void Clear()
    {
        objects.Clear();

        if (divided)
        {
            northWest.Clear();
            northEast.Clear();
            southWest.Clear();
            southEast.Clear();
        }

        divided = false;
    }
   
    public void OnDrawGizmos()
    {
        // Draw a wire cube for the boundary of this node
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(boundary.center, boundary.size);

        // If this node is divided, draw the child nodes
        if (divided)
        {
            northWest.OnDrawGizmos();
            northEast.OnDrawGizmos();
            southWest.OnDrawGizmos();
            southEast.OnDrawGizmos();
        }
    }
    
}