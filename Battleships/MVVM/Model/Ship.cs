using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleships.MVVM.Enums;
using Battleships.MVVM.Model.DataTransferObjects;
using Battleships.MVVM.ViewModel;

namespace Battleships.MVVM.Model
{
    /// <summary>
    /// Defines a contract for modelling a ship in a battleships game. It sets the required properties to 
    /// ensure that the <see cref="Ship"/> class contains the necessary information to run a game.
    /// </summary>
    public interface IShip
    {
        ShipType ShipType { get; }
        int Size { get; }
        bool[] Damage { get; set; }
        int[] Positions { get; }
        bool IsHorizontal { get; }
        bool IsUndamaged { get; }
        bool IsSunk { get; }
        bool CheckForHit(int position);
    }

    /// <summary>
    /// A base class that implements the <see cref="IBoard"/> interface. It is used for representing the ships 
    /// in a Battleships game. It contains key information about the position and state of the ship. It provides 
    /// methods for checking if the ship has been hit and updating its damage status if needed. This is used to 
    /// pass accurate information to the <see cref="Game"/> class and <see cref="PlayGameViewModel"/> for 
    /// reliable game management and user displays.
    /// </summary>
    public abstract class Ship : IShip
    {
        #region Properties
        public abstract ShipType ShipType { get; }
        public virtual int Size { get; }
        public bool[] Damage { get; set; }
        public int[] Positions { get; }
        public bool IsHorizontal { get; }
        public bool IsUndamaged => Damage.All(damage => damage == false);
        public bool IsSunk => Damage.All(damage => damage == true);
        #endregion //Properties

        /// <summary>
        /// The primary constructor used to instantiate ships during a new game set up.
        /// </summary>
        /// <param name="shipPositions">A tuple containing the integer value of the starting grid position 
        /// for the ship and the boolean value indicating if it is horizontal.</param>
        /// <param name="size">An integer value representing the size of the ship.</param>
        public Ship((int startPosition, bool isHorizontal) shipPositions, int size)
        {
            Size = size;
            
            Damage = new bool[Size];
            Positions = new int[Size];

            IsHorizontal = shipPositions.isHorizontal;

            for (int i = 0; i < Size; i++) //Adding ten moves down a row
                Positions[i] = IsHorizontal ? shipPositions.startPosition + i : shipPositions.startPosition + 10 * i;
        }

        /// <summary>
        /// <see cref="ShipDTO"/> used to load previous game state data during the load game process.
        /// </summary>
        /// <param name="shipDTO">A <see cref="ShipDTO"/> containing loaded information to return to a 
        /// previous game state.</param>
        public Ship(ShipDTO shipDTO)
        {
            Size = shipDTO.Size;
            Damage = shipDTO.Damage;
            Positions = shipDTO.Positions;
            IsHorizontal = shipDTO.IsHorizontal;
        }

        #region Methods
        /// <summary>
        /// Checks if the ship has been hit by an attack, returning a boolean value to represent the result. 
        /// Updates the <see cref="Damage"/> property if necessary. Used as part of the shot evaluation process 
        /// during a game of Battleships.
        /// </summary>
        /// <param name="position">An integer representing the grid position selected for attack.</param>
        /// <returns>A boolean value indicating whether the ships has been hit.</returns>
        public bool CheckForHit(int position)
        {
            if (Positions.Contains(position))
            {
                UpdateDamage(position);
                return true;
            }
                return false;
        }

        /// <summary>
        /// Updates the <see cref="Damage"/> array in the event that the ship is hit. Enables the ship to 
        /// accurately keep track of its condition to support game management.
        /// </summary>
        /// <param name="position">An integer representing the grid position that has been hit.</param>
        protected void UpdateDamage(int position)
        {
            int damageIndex = Array.IndexOf(Positions, position);
            Damage[damageIndex] = true;
        }

        /// <summary>
        /// Generates a <see cref="ShipDTO"/> for Json serialization to ensure simple data storage.
        /// </summary>
        /// <returns>A <see cref="ShipDTO"/> instance containing key information in its 
        /// current state.</returns>
        public ShipDTO GetDTO()
        {
            return new ShipDTO()
            {
                ShipType = this.ShipType,
                Size = this.Size,
                Damage = this.Damage,
                Positions = this.Positions,
                IsHorizontal = this.IsHorizontal
            };
        }
        #endregion //Methods
    }
}
