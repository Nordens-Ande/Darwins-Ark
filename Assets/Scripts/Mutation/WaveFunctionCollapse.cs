using NUnit.Framework;
using System.Collections.Generic;
//using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;

public enum WFCDomenant 
{ 
    fatherDominent,
    motherDominent, 
    perfectMix
}

public class WFCcell 
{
    private bool isCollapsed = false;
    private int entropy = 0;
    private Color cellColor;
    private WFCDomenant cellsDominentParent;
    private List<WFCDomenant> activeChoices; //= new List<WFCDomenant> {WFCDomenant.fatherDominent, WFCDomenant.motherDominent, WFCDomenant.perfectMix };
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
    private int textureSize = 64;
    Texture2D fatherTexture;
    Texture2D motherTexture;
    Texture2D newTexture;
    List<WFCcell> isnetCollapsedCells = new List<WFCcell>();

    WFCcell[,] cellsArray;

    List<Color> possibleColorsFather;
    List<Color> possibleColorsMother;
    List <Color> possibleColorsPerfectMix; // Will act as a buffert between the dominent forces

    //DebugValues
    [SerializeField] private GameObject debugObj;
    [SerializeField] private bool activateDebug;

    void GetPossibleColors(Texture2D fatherTex, Texture2D motherTex, WFCcell[,] cells, List<WFCcell> isnetCollapsed) 
    {

        possibleColorsFather = new List<Color>();
        possibleColorsMother = new List<Color>();
        possibleColorsPerfectMix = new List<Color>(); // Will act as a buffert between the dominent forces

        Color[] fatherPixels = fatherTex.GetPixels();
        Color[] motherPixels = motherTex.GetPixels();

        //cells = new WFCcell[fatherTex.width, fatherTex.height];

        cellsArray = new WFCcell[fatherTex.width, fatherTex.height];

        PopulateGridWithCells(cells, fatherTex.width, fatherTex.height, isnetCollapsed);

        for(int i = 0; i < fatherPixels.Length; i++) 
        {
            if (!possibleColorsFather.Contains(fatherPixels[i]))
            {
                possibleColorsFather.Add(fatherPixels[i]);
            }

        }

        for (int i = 0; i < motherPixels.Length; i++)
        {
            if (!possibleColorsMother.Contains(motherPixels[i]))
            {
                possibleColorsMother.Add(motherPixels[i]);
            }
        }

        //This will add a third list that acts as a buffer between the two different colors
        for (int i = 0; i < possibleColorsMother.Count; i++) 
        {
            if (possibleColorsFather[i] == null) return;
            possibleColorsPerfectMix.Add(Color.Lerp(possibleColorsMother[i], possibleColorsFather[i], Random.Range(0.3f, 0.6f))); //Adds some variaty to the streangth
        }
    }

    void PopulateGridWithCells(WFCcell[,] array, int width, int height, List<WFCcell> isnetCollapsedCells) 
    {
        WFCDomenant father = WFCDomenant.fatherDominent;
        WFCDomenant mother = WFCDomenant.fatherDominent;
        WFCDomenant mix = WFCDomenant.fatherDominent;
        List<WFCDomenant> list = new List<WFCDomenant> { father, mother, mix };

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                WFCcell cell = new WFCcell();
                cell.GridPosX = i;
                cell.GridPosY = j;
                cell.ActiveChoices = new List<WFCDomenant> {WFCDomenant.fatherDominent, WFCDomenant.motherDominent, WFCDomenant.perfectMix };
                //cell.CellsDominentParent = null;
                cell.Entropy = list.Count;
                cell.IsCollapsed = false;
                isnetCollapsedCells.Add(cell);

                array[i, j] = cell;
            }
        }
    }

    //Will be used to initionaly populate the textures
    Color GetRandomColorFromList(List<Color> list) 
    {
        Color color = new Color();
        color = list[Random.Range(0, list.Count)];
        return color;
    }

    WFCcell GetLowestEntropy(WFCcell[,] cells, List<WFCcell> isnetCollapsedCells, int width, int height)
    {
        if (isnetCollapsedCells.Count == 0) return null;

        WFCcell tempcell = isnetCollapsedCells[0];
        foreach (WFCcell cell in isnetCollapsedCells)
        {
            if (tempcell == cell || cell.IsCollapsed) continue;
            if (cell.Entropy < tempcell.Entropy)
            {
                tempcell = cell;
            }
        }
        return tempcell;
    }



    //    WFCcell tempCell = cells[(width/ 2),(height/2)];
    //    foreach(WFCcell cell in cells) 
    //    {
    //        if (tempCell == cell || cell.IsCollapsed) continue;
    //        if(cell.Entropy < tempCell.Entropy) 
    //        {
    //            tempCell = cell;
    //        }
    //    }
    //    return tempCell;
    //}

    void CollapseCell(WFCcell cell, List<WFCcell> insetCollapsedCells) 
    {
        if(cell == null) 
        {
            insetCollapsedCells.Remove(cell);
            return;
        }

        if(cell.ActiveChoices.Count == 1) 
        {
            cell.CellsDominentParent = cell.ActiveChoices[0];
            cell.IsCollapsed = true;
            insetCollapsedCells.Remove(cell);
        }
        else 
        {
            cell.CellsDominentParent = cell.ActiveChoices[Random.Range(0, cell.ActiveChoices.Count)];
            cell.IsCollapsed = true;
            insetCollapsedCells.Remove(cell);
        }

    }

    void RulesOfNeighbor(WFCcell cell, int width, int height, WFCcell[,] cells, List<WFCcell> isnetcollapsedCell) //Propegering?
    {
        if (cell.IsCollapsed) return;

        WFCDomenant selectedDomenent = cell.CellsDominentParent;
        List<WFCcell> neighbor = new List<WFCcell>();
        if (cell.GridPosX < width -1 && cell.GridPosY < height - 1 && cell.GridPosX > 0 && cell.GridPosY > 0) 
        {
            for (int i = -1; i < 1; i++)
            {
                for (int j = -1; j < 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    WFCcell currentCell = cells[cell.GridPosX + i, cell.GridPosY + j];
                    if (!currentCell.IsCollapsed)
                    {
                        neighbor.Add(cells[cell.GridPosX + i, cell.GridPosY + j]);
                    }
                }
            }
        }

        if (neighbor.Count > 0)
        {
            if (cell.CellsDominentParent == WFCDomenant.motherDominent)
            {
                foreach (WFCcell wFCcell in neighbor)
                {
                    if (!wFCcell.IsCollapsed)
                    {
                        wFCcell.ActiveChoices.Remove(WFCDomenant.fatherDominent);
                        wFCcell.Entropy = wFCcell.ActiveChoices.Count;
                        //CollapseCell(wFCcell, isnetcollapsedCell);
                    }
                }
            }
            else if (cell.CellsDominentParent == WFCDomenant.fatherDominent)
            {
                foreach (WFCcell wFCcell in neighbor)
                {
                    if (!wFCcell.IsCollapsed)
                    {
                        wFCcell.ActiveChoices.Remove(WFCDomenant.motherDominent);
                        wFCcell.Entropy = wFCcell.ActiveChoices.Count;
                        //CollapseCell(wFCcell, isnetcollapsedCell);
                    }
                }
            }
            //else if (cell.CellsDominentParent == WFCDomenant.perfectMix)
            //{
            //    foreach (WFCcell wFCcell in neighbor)
            //    {
            //        if (!wFCcell.IsCollapsed)
            //        {
            //            wFCcell.ActiveChoices.Remove(WFCDomenant.perfectMix);
            //            wFCcell.Entropy = wFCcell.ActiveChoices.Count;
            //            //CollapseCell(wFCcell, isnetcollapsedCell);
            //        }
            //    }

            //}
        }
        //else 
        //{ 

        //    return;
        //}
    }

    public void WFC(Texture2D fatherTexture, Texture2D motherTexture, WFCcell[,] cells)
    {
        GetPossibleColors(fatherTexture, motherTexture, cells, isnetCollapsedCells);
        //PopulateCellChoices(cells);

        while(isnetCollapsedCells.Count != 0) 
        {
            WFCcell cell = GetLowestEntropy(cells, isnetCollapsedCells, fatherTexture.width, fatherTexture.height);
            if (cell != null) 
            {
                CollapseCell(cell, isnetCollapsedCells);
                RulesOfNeighbor(cell, fatherTexture.width, fatherTexture.height, cellsArray, isnetCollapsedCells);
            }
            else 
            {
                isnetCollapsedCells.Clear(); //Tried to Failsafe
            }
        }

        PaintTheFinalResult(cells, fatherTexture.width, fatherTexture.height);
        CreateNewTexture(newTexture, cells);

    }


    void PaintTheFinalResult(WFCcell[,] cells, int width, int height) 
    {

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {

                //cells[i, j].CellColor = GetRandomColorFromList(possibleColorsMother);
                if (cells[i, j].CellsDominentParent == WFCDomenant.motherDominent)
                {
                    //cells[i, j].CellColor = GetRandomColorFromList(possibleColorsMother);
                    cells[i, j].CellColor = possibleColorsMother[0];
                }
                else if (cells[i, j].CellsDominentParent == WFCDomenant.fatherDominent)
                {
                    //cells[i, j].CellColor = GetRandomColorFromList(possibleColorsFather);
                    cells[i, j].CellColor = possibleColorsFather[0];
                }
                else if(cells[i, j].CellsDominentParent == WFCDomenant.perfectMix)
                {
                    //cells[i, j].CellColor = GetRandomColorFromList(possibleColorsPerfectMix);
                    cells[i, j].CellColor = possibleColorsPerfectMix[0];
                }

            }
        }
    }

    void CreateNewTexture(Texture2D size, WFCcell[,] cells) 
    {
        newTexture = new Texture2D(size.width, size.height);
        for (int i = 0; i < size.width; i ++) 
        { 
            for (int j = 0; j < size.height; j++) 
            {
                Color setColor = cells[i, j].CellColor;
                newTexture.SetPixel(i, j, setColor);
            }
        }
        newTexture.Apply();
    }

    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// <param name="fatherTex"></param>
    /// <param name="motherTex"></param>
    /// 

    //Debug feature, making textures inside this scripts to see if it works
    void DebugCreateTextures(Texture2D fatherTex, Texture2D motherTex) 
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);

        for(int i = 0; i < texture.width; i++) 
        { 
            for (int j = 0; j < texture.height; j++) 
            {
                fatherTex.SetPixel(i, j, Color.blue);
                motherTex.SetPixel(i, j, Color.red);
            }
        }
        fatherTex.Apply();
        motherTex.Apply();
    }

    Texture2D DebugCreateFather() 
    { 
        Texture2D fatherTex = new Texture2D(textureSize,textureSize);
        return fatherTex;
    }
    Texture2D DebugCreateMother()
    {
        Texture2D motherTex = new Texture2D(textureSize, textureSize);
        return motherTex;
    }

    void DebugSetobjColor(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        newTexture.filterMode = FilterMode.Point;
        renderer.material.mainTexture = newTexture;
    }

    public void DebugWFC()
    {
        List<WFCcell> isnetCollapsedCells = new List<WFCcell>();
        Texture2D fatherTex = DebugCreateFather();
        Texture2D motherTex = DebugCreateMother();
        DebugCreateTextures(fatherTex, motherTex);

        WFCcell[,] cells = new WFCcell[fatherTex.width, fatherTex.height];
        GetPossibleColors(fatherTex, motherTex, cells, isnetCollapsedCells);

        //Failsafe
        int currentIteration = 0;
        int maxIteration = fatherTex.width * fatherTex.height;

        while (isnetCollapsedCells.Count != 0 && currentIteration < maxIteration)
        {
            currentIteration++;
            WFCcell cell = GetLowestEntropy(cells, isnetCollapsedCells, fatherTex.width, fatherTex.height);
            CollapseCell(cell, isnetCollapsedCells);
            RulesOfNeighbor(cell, fatherTex.width, fatherTex.height, cells, isnetCollapsedCells);
        }

        Debug.Log(isnetCollapsedCells.Count);

        if (isnetCollapsedCells.Count != 0) 
        { 
            foreach(WFCcell cell in isnetCollapsedCells) 
            {
                cell.CellColor = possibleColorsMother[0];
            }
        }

        PaintTheFinalResult(cells, fatherTex.width, fatherTex.height);
        CreateNewTexture(fatherTex, cells);
        DebugSetobjColor(debugObj);

    }

    private void Update()
    {
        if (activateDebug == true)
        {
            activateDebug = false;
            DebugWFC();
        }
    }
}
