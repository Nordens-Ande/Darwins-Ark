using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public enum WFCDomenant 
{ 
    fatherDominent,
    motherDominent, 
    perfectMix
}

public class WFCcell 
{
    private bool isCollapsed;
    private int entropy;
    private Color cellColor;
    private WFCDomenant cellsDominentParent;

    public bool IsCollapsed 
    { 
        get => isCollapsed;
        set => isCollapsed = value;
    }
    public int Entropy 
    { 
        get => entropy;
        set => entropy = value;
    }
    public Color CellColor
    {
        get => cellColor;
        set => cellColor = value;
    }

    public WFCDomenant CellCellsDominentParent 
    {
        get => cellsDominentParent;
        set => cellsDominentParent = value;
    }
}

public class WaveFunctionCollapse : MonoBehaviour
{
    Texture2D fatherTexture;
    Texture2D motherTexture;

    WFCcell[,] cells;


    List<Color> possibleColorsFather = new List<Color>();
    List<Color> possibleColorsMother = new List<Color>();
    List <Color> possibleColorsPerfectMix = new List<Color>(); // Will act as a buffert between the dominent forces

    void GetPossibleColors(Texture2D fatherTex, Texture2D motherTex) 
    {
        Color[] fatherPixels = fatherTex.GetPixels();
        Color[] motherPixels = motherTex.GetPixels();

        cells = new WFCcell[fatherTex.width, fatherTex.height];

        foreach (Color color in fatherPixels)
        {
            if (possibleColorsFather.Contains(color)) 
            { 
                possibleColorsFather.Add(color);
            }
        }

        foreach (Color color in motherPixels) 
        {
            if (possibleColorsMother.Contains(color)) 
            { 
                possibleColorsMother.Add(color);
            }
        }

        //This will add a third list that acts as a buffer between the two different colors
        for (int i = 0; i < possibleColorsMother.Count; i++) 
        {
            if (possibleColorsFather[i] == null) return;
            possibleColorsPerfectMix.Add(Color.Lerp(possibleColorsMother[i], possibleColorsFather[i], Random.Range(0.3f, 0.6f))); //Adds some variaty to the streangth
        }

    }

    //Will be used to initionaly populate the textures
    Color GetRandomColorFromList(List<Color> list) 
    {
        return list[Random.Range(0,list.Count)];
    }

    void GiveStartEntropy() 
    { 
        foreach (WFCcell cell in cells) 
        {
            cell.Entropy = 2;
        }
        
    }

    void GetLowestEntropy() 
    {
        WFCcell tempCell = cells[0,0];
        foreach(WFCcell cell in cells) 
        {
            if (tempCell == cell) continue;
            if(cell.Entropy < tempCell.Entropy) 
            {
                tempCell = cell;
            }
        }
    }

    public void WFC()
    {
    
    }

}
