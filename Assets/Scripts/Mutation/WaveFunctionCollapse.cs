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
    private List<WFCDomenant> activeChoices;
    private int gridPosX;
    private int gridPosY;

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

    public List<WFCDomenant> ActiveChoices 
    { 
        get => activeChoices;
        set => activeChoices = value;
    }

    public WFCDomenant CellsDominentParent 
    {
        get => cellsDominentParent;
        set => cellsDominentParent = value;
    }

    public int GridPosX 
    { 
        get => gridPosX;
        set => gridPosX = value;
    }
    public int GridPosY 
    {
        get => gridPosY;
        set => gridPosY = value;
    }
}

public class WaveFunctionCollapse : MonoBehaviour
{
    Texture2D fatherTexture;
    Texture2D motherTexture;
    List<WFCcell> isnetCollapsedCells = new List<WFCcell>();

    WFCcell[,] cells;


    List<Color> possibleColorsFather = new List<Color>();
    List<Color> possibleColorsMother = new List<Color>();
    List <Color> possibleColorsPerfectMix = new List<Color>(); // Will act as a buffert between the dominent forces

    void GetPossibleColors(Texture2D fatherTex, Texture2D motherTex) 
    {
        Color[] fatherPixels = fatherTex.GetPixels();
        Color[] motherPixels = motherTex.GetPixels();

        cells = new WFCcell[fatherTex.width, fatherTex.height];

        PopulateGridWithCells(cells, fatherTex.width, fatherTex.height);

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

    void PopulateGridWithCells(WFCcell[,] array, int width, int height) 
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; i++)
            {
                WFCcell cell = new WFCcell();
                cell.GridPosX = i;
                cell.GridPosY = j;
                cells[i, j] = cell;
            }
        }
    }

    void PopulateCellChoices(WFCcell[,] cells) 
    {
        List<WFCDomenant> list = new List<WFCDomenant> {WFCDomenant.fatherDominent, WFCDomenant.motherDominent, WFCDomenant.perfectMix };
        foreach (WFCcell cell in cells) 
        { 
            cell.ActiveChoices = list;
            cell.Entropy = list.Count;
            isnetCollapsedCells.Add(cell); //Needed to check that all cells are collapsed

        }
    }

    //Will be used to initionaly populate the textures
    Color GetRandomColorFromList(List<Color> list) 
    {
        return list[Random.Range(0,list.Count)];
    }

    WFCcell GetLowestEntropy() 
    {
        if (isnetCollapsedCells.Count == 0) return null;

        WFCcell tempCell = cells[0,0];
        foreach(WFCcell cell in cells) 
        {
            if (tempCell == cell || cell.IsCollapsed) continue;
            if(cell.Entropy < tempCell.Entropy) 
            {
                tempCell = cell;
            }
        }
        return tempCell;
    }

    void CollapseCell(WFCcell cell) 
    {
        cell.CellsDominentParent = cell.ActiveChoices[Random.Range(0, cell.ActiveChoices.Count)];
        cell.IsCollapsed = true;
        isnetCollapsedCells.Remove(cell);

    }

    void RulesOfNeighbor(WFCcell cell) //Propegering?
    {
        if (cell.IsCollapsed) return;

        WFCDomenant selectedDomenent = cell.CellsDominentParent;
        List<WFCcell> neighbor = new List<WFCcell>();

        for (int i = -1; i < 1; i++) 
        {
            for (int j = -1; j < 1; j++) 
            {
                if (i == 0 && j == 0) return;

                WFCcell currentCell = cells[cell.GridPosX + i, cell.GridPosY + j];
                if (!currentCell.IsCollapsed)
                { 
                    neighbor.Add(cells[cell.GridPosX + i, cell.GridPosY + j]); 
                }
            }
        }

        if(cell.CellsDominentParent == WFCDomenant.motherDominent) 
        { 
            foreach(WFCcell wFCcell in neighbor) 
            {
                wFCcell.ActiveChoices.Remove(WFCDomenant.fatherDominent);
                wFCcell.Entropy = wFCcell.ActiveChoices.Count;
            }
        }
        else if(cell.CellsDominentParent == WFCDomenant.fatherDominent) 
        {
            foreach (WFCcell wFCcell in neighbor)
            {
                wFCcell.ActiveChoices.Remove(WFCDomenant.motherDominent);
                wFCcell.Entropy = wFCcell.ActiveChoices.Count;
            }
        }
    }

    public void WFC(Texture2D fatherTexture, Texture2D motherTexture, WFCcell[,] cells)
    {
        GetPossibleColors(fatherTexture, motherTexture);
        PopulateCellChoices(cells);

        while(isnetCollapsedCells.Count != 0) 
        {
            WFCcell cell = GetLowestEntropy();
            if (cell != null) 
            {
                CollapseCell(cell);
                RulesOfNeighbor(cell);
            }
            else 
            {
                isnetCollapsedCells.Clear(); //Tried to Failsafe
            }
        }

        PaintTheFinalResult(cells);
        CreateNewTexture();

    }


    void PaintTheFinalResult(WFCcell[,] cells ) 
    { 
        foreach(WFCcell cell in cells) 
        { 
            if(cell.CellsDominentParent == WFCDomenant.motherDominent) 
            {
                cell.CellColor = GetRandomColorFromList(possibleColorsMother);
            }
            else if(cell.CellsDominentParent == WFCDomenant.fatherDominent) 
            {
                cell.CellColor = GetRandomColorFromList(possibleColorsFather);
            }
            else 
            {
                cell.CellColor = GetRandomColorFromList(possibleColorsPerfectMix);
            }
        }
    }

    void CreateNewTexture() 
    { 
    
    }

}
