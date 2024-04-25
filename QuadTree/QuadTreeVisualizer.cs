using System.Collections.Generic;
using UnityEngine;


public class QuadTreeVisualizer : MonoBehaviour
{
    public QuadTreeNode<GameObject> quadTree;

    void OnDrawGizmos()
    {
        
        
        if (quadTree != null)
        {
            quadTree.OnDrawGizmos();
        }
        
        
    }
    
    
}