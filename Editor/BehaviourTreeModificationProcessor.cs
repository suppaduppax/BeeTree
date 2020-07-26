using System.Collections;
using System.Collections.Generic;
using System.IO;
using BeeTree;
using UnityEditor;
using UnityEngine;

public class BehaviourTreeModificationProcessor : UnityEditor.AssetModificationProcessor
{
    private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
    {
        Debug.Log("TEST");
        var behaviourTree = AssetDatabase.LoadMainAssetAtPath(sourcePath) as BehaviourTree;
        
        if (behaviourTree == null)
        {
            return AssetMoveResult.DidNotMove;
        }

        var srcDir = Path.GetDirectoryName(sourcePath);
        var dstDir = Path.GetDirectoryName(destinationPath);

        if (srcDir != dstDir)
        {
            return AssetMoveResult.DidNotMove;
        }

        var originalFilename = Path.GetFileNameWithoutExtension(sourcePath);
        var filename = Path.GetFileNameWithoutExtension(destinationPath);
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);

        string tryName = filename;
        
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] == behaviourTree)
            {
                continue;;
            }

            if (assets[i].name == filename)
            {
                Debug.LogError("Cannot rename BehaviourTree, an existing subasset with that name already exists!");
                behaviourTree.name = originalFilename;
                return AssetMoveResult.FailedMove;
            }
        }
        
        behaviourTree.name = filename;

        return AssetMoveResult.DidNotMove;
    }
    
}