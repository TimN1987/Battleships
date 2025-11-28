using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.ViewModel.Base;
using Battleships.MVVM.View;
using System.Diagnostics;

namespace Battleships.MVVM.ViewModel.GridCells;

public class PlayGameGridCell(int row, int column, ThemeNames theme) : GridCellBase
{
    #region Theme Image Resources
    private static readonly ThemeNames[] DarkBackgroundThemes =
    [
        ThemeNames.Dark,
    ];

    private static readonly ThemeNames[] NeonBackgroundThemes =
    [
        ThemeNames.Neon
    ];

    private static readonly ThemeNames[] LightBackgroundThemes =
    [
        ThemeNames.Classic,
        ThemeNames.Light,
        ThemeNames.Neutral,
    ];

    private readonly Uri _classicLightMiss = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/classiclightmiss.png", UriKind.Absolute);
    private readonly Uri _darkMiss = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/darkmiss.png", UriKind.Absolute);
    private readonly Uri _neonMiss = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/neonmiss.png", UriKind.Absolute);
    private readonly Uri _neutralMiss = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/neutralmiss.png", UriKind.Absolute);

    private readonly Uri _classicExplosion = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/explosion.png", UriKind.Absolute);
    private readonly Uri _darkExplosion = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/explosiondark.png", UriKind.Absolute);
    private readonly Uri _neonExplosion = new(@"pack://application:,,,/MVVM/Resources/Images/PlayGameView/explosionneon.png", UriKind.Absolute);
    #endregion //Theme Image Resources

    #region Fields
    private bool _isHighlighted = (row == 0 && column == 0);
    private bool _isTouching = false;
    private GridCellState _cellState = GridCellState.Unattacked;
    private ThemeNames _theme = theme;
    private Uri? _displayImage;
    #endregion //Fields

    #region Properties
    public bool IsHighlighted
    {
        get => _isHighlighted;
        set => SetProperty(ref _isHighlighted, value);
    }
    public bool IsTouching
    {
        get => _isTouching;
        set => SetProperty(ref _isTouching, value);
    }
    public GridCellState CellState
    {
        get => _cellState;
        set => SetProperty(ref _cellState, value);
    }
    public Uri? DisplayImage
    {
        get => _displayImage;
        set => SetProperty(ref _displayImage, value);
    }
    public int Row { get; set; } = row;
    public int Column { get; set; } = column;
    public int Index => 10 * Row + Column;
    #endregion //Properties

    /// <summary>
    /// Sets the <see cref="IsOccupied"/>, <see cref="IsTouching"/> and <see cref="IsHighlighted"/> 
    /// properties to the given boolean value <paramref name="isHighlighted"/>. Used to update the 
    /// cell highlighting during ship placement.
    /// </summary>
    /// <param name="highlightingType">The <see cref="CellHighlighting"/> type to be updated.</param>
    /// <param name="isHighlighted">A boolean value indicating the new value for the <see 
    /// cref="CellHighlighting"/>.</param>
    public void SetHighlighting(CellHighlighting highlightingType, bool isHighlighted)
    {
        switch (highlightingType)
        {
            case CellHighlighting.IsHighlighted:
                IsHighlighted = isHighlighted;
                break;

            case CellHighlighting.IsTouching:
                IsTouching = isHighlighted;
                break;
        }
    }

    /// <summary>
    /// Updates the <see cref="GridCellState"/> of the cell after an attack has been recorded on that 
    /// cell. Ensures that the <see cref="PlayGameView"/> can display the correct graphics.
    /// </summary>
    /// <param name="newState">The new <see cref="GridCellState"/> value for the cell.</param>
    public void UpdateCellState(GridCellState newState)
    {
        CellState = newState;

        DisplayImage = CellState switch
        {
            GridCellState.Miss => _theme == ThemeNames.Dark ? _darkMiss :
                                    _theme == ThemeNames.Neon ? _neonMiss : 
                                    _theme == ThemeNames.Neutral ? _neutralMiss : 
                                    _classicLightMiss,
            GridCellState.Hit => LightBackgroundThemes.Contains(_theme) ? _classicExplosion :
                                 DarkBackgroundThemes.Contains(_theme) ? _darkExplosion :
                                 _neonExplosion,
            _ => null
        };
    }

    /// <summary>
    /// Updates the stored theme value to ensure that the correct image is displayed.
    /// </summary>
    public void UpdateTheme(ThemeNames newTheme)
    {
        _theme = newTheme;

        DisplayImage = CellState switch
        {
            GridCellState.Miss => _theme == ThemeNames.Dark ? _darkMiss :
                                    _theme == ThemeNames.Neon ? _neonMiss :
                                    _theme == ThemeNames.Neutral ? _neutralMiss :
                                    _classicLightMiss,
            GridCellState.Hit => LightBackgroundThemes.Contains(_theme) ? _classicExplosion :
                                 DarkBackgroundThemes.Contains(_theme) ? _darkExplosion :
                                 _neonExplosion,
            _ => null
        };
    }
}

