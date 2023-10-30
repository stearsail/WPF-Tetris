
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Tetris_game.Models;

namespace Tetris_game.ViewModels
{
    public class MainViewModel : BaseModel
    {
        private Models.Block activeBlock;
        private Models.Block heldBlock;
        private BlockQueue blockQueue = new BlockQueue();
        private ObservableCollection<ObservableCollection<(bool, Color)>> occupiedCells;
        private bool isRunning = false;
        private bool canHoldBlock = true;

        private ICommand startGameCommand;
        private ICommand moveDownCommand;
        private ICommand moveRightCommand;
        private ICommand moveLeftCommand;
        private ICommand rotateCWCommand;
        private ICommand rotateCCWCommmand;
        private ICommand holdBlockCommand;
       
        public ObservableCollection<ObservableCollection<(bool, Color)>> OccupiedCells
        {
            get=>occupiedCells;
            set
            {
                occupiedCells = value;
                OnPropertyChanged();
            }
        }
        public Models.Block ActiveBlock
        {
            get => activeBlock;
            set
            {
                activeBlock = value;
                OnPropertyChanged();
            }
        }

        public Models.Block HeldBlock
        {
            get => heldBlock;
            set
            {
                heldBlock = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartGameCommand
        {
            get
            {
                if (startGameCommand == null)
                {
                    startGameCommand = new RelayCommand(_ => StartGame(), _=>!isRunning);
                }
                return startGameCommand;
            }
            
        }
        public ICommand MoveDownCommand
        {
            get
            {
                if (moveDownCommand == null)
                {
                    moveDownCommand = new RelayCommand(_ => MoveDown(), _ => isRunning);
                }
                return moveDownCommand;
            }
            
        }
        
        public ICommand MoveRightCommand
        {
            get
            {
                if (moveRightCommand == null)
                {
                    moveRightCommand = new RelayCommand(_ => MoveRight(), _ => isRunning);
                }
                return moveRightCommand;
            }
            
        } 
        
        public ICommand MoveLeftCommand
        {
            get
            {
                if (moveLeftCommand == null)
                {
                    moveLeftCommand = new RelayCommand(_ => MoveLeft(), _ => isRunning);
                }
                return moveLeftCommand;
            }
            
        }   
        
        public ICommand RotateCWCommand
        {
            get
            {
                if (rotateCWCommand == null)
                {
                    rotateCWCommand = new RelayCommand(_ => RotateCW(), _ => isRunning);
                }
                return rotateCWCommand;
            }
            
        }  
        
        public ICommand RotateCCWCommand
        {
            get
            {
                if (rotateCCWCommmand == null)
                {
                    rotateCCWCommmand = new RelayCommand(_ => RotateCCW(), _ => isRunning);
                }
                return rotateCCWCommmand;
            }
            
        }                
        public ICommand HoldBlockCommand
        {
            get
            {
                if(holdBlockCommand == null)
                {
                    holdBlockCommand = new RelayCommand(_ => HoldBlock(), _ => isRunning);
                }
                return holdBlockCommand;
            }
            
        }        

        private bool BlockFits(int row, int col) 
        {
            
            foreach(Position pos in ActiveBlock.PossiblePositions[ActiveBlock.CurrentPosition])
            {
                int i = pos.Row + ActiveBlock.Offset.Row + row;
                int j = pos.Column + ActiveBlock.Offset.Column + col;


                if ( i > 21 || j<0 || j > 9 || OccupiedCells[i][j].Item1)
                    return false;
                
                
            }
            return true;
        }

        private void SavePreviousPosition()
        {
            ObservableCollection<Position> positions = new ObservableCollection<Position>();
            ActiveBlock.PreviousPosition.Clear();
            foreach (Position p in ActiveBlock.PossiblePositions[ActiveBlock.CurrentPosition])
            {
                positions.Add(new Position(p.Row + ActiveBlock.Offset.Row, p.Column + ActiveBlock.Offset.Column));
            }
            ActiveBlock.PreviousPosition = positions;
        }

        private void MoveDown()
        {
            if (BlockFits(1,0))
            {
                SavePreviousPosition();
                ActiveBlock.Offset.Row += 1;
            }
            else
                PlaceBlock();
        }

        private void MoveRight()
        {
            if(BlockFits(0,1))
            {
                SavePreviousPosition();
                ActiveBlock.Offset.Column += 1;
            }
        }

        private void MoveLeft()
        {
            if(BlockFits(0,-1))
            {
                SavePreviousPosition();
                ActiveBlock.Offset.Column -= 1;
            }
        }

        private void RotateCW()
        {
            if (RotationExitsBoundaries(true))
            {
                TryFitRotation(true);
                return;
            }
            SavePreviousPosition();
            ActiveBlock.CurrentPosition = GetRotationIndex(true);
        }

        private void RotateCCW()
        {
            if (RotationExitsBoundaries(false))
            {
                TryFitRotation(false);
                return;
            }
            SavePreviousPosition();
            ActiveBlock.CurrentPosition = GetRotationIndex(false);
        }

        private int GetRotationIndex(bool CW)
        {
            if (CW)
            {
                if (ActiveBlock.CurrentPosition == ActiveBlock.PossiblePositions.Count - 1)
                {
                   return 0;
                }
                else
                   return ActiveBlock.CurrentPosition + 1;
            }
            else
            {
                if (ActiveBlock.CurrentPosition == 0)
                {
                    return ActiveBlock.PossiblePositions.Count - 1;
                }
                else
                    return ActiveBlock.CurrentPosition - 1;
            }
        }

        private bool RotationExitsBoundaries(bool CW)
        {
            foreach (Position pos in ActiveBlock.PossiblePositions[GetRotationIndex(CW)])
            {
                int i = pos.Row + ActiveBlock.Offset.Row;
                int j = pos.Column + ActiveBlock.Offset.Column;
                if (i < 0 || i > 21 ||
                    j > 9 || j < 0 || OccupiedCells[i][j].Item1)
                {
                    return true;
                }
            }
            return false;
        }

        private void TryFitRotation(bool CW)
        {
            int maxColumn = 9;
            int minColumn = 0;
            int off = 0;
            foreach(Position pos in ActiveBlock.PossiblePositions[GetRotationIndex(CW)])
            { 
                int i = pos.Row + ActiveBlock.Offset.Row;
                int j = pos.Column + ActiveBlock.Offset.Column;
                maxColumn = Math.Max(j, maxColumn);
                minColumn = Math.Min(j, minColumn);
            }
            if (maxColumn != 9)
            {
                off = maxColumn - 9;
                foreach(Position p in ActiveBlock.PossiblePositions[GetRotationIndex(CW)])
                {
                    int i = p.Row + ActiveBlock.Offset.Row;
                    int j = p.Column + ActiveBlock.Offset.Column;
                    if (OccupiedCells[i][j - off].Item1)
                        return;
                }
                SavePreviousPosition();
                ActiveBlock.Offset.Column -= off;
                SavePreviousPosition();
                ActiveBlock.CurrentPosition = GetRotationIndex(CW);
            } 
            else if (minColumn != 0)
            {
                off = Math.Abs(minColumn);
                foreach (Position p in ActiveBlock.PossiblePositions[GetRotationIndex(CW)])
                {
                    int i = p.Row + ActiveBlock.Offset.Row;
                    int j = p.Column + ActiveBlock.Offset.Column;
                    if (OccupiedCells[i][j + off].Item1)
                        return;
                }
                SavePreviousPosition();
                ActiveBlock.Offset.Column += off;
                SavePreviousPosition();
                ActiveBlock.CurrentPosition = GetRotationIndex(CW);
            }
            else
            {
                int collisions = 0;
                int medianColumn = 0;
                HashSet<int> collidedColumns = new HashSet<int>();

                foreach(Position pos in ActiveBlock.PossiblePositions[GetRotationIndex(CW)])
                {
                    int i = pos.Row + ActiveBlock.Offset.Row;
                    int j = pos.Column + ActiveBlock.Offset.Column;
                    if (OccupiedCells[i][j].Item1)
                    {
                        if (!collidedColumns.Contains(j))
                        {
                            collisions++;
                            collidedColumns.Add(j);
                        }
                    }
                    medianColumn += j;
                }
                medianColumn /= 4;
                if(collidedColumns.ElementAt(0) < medianColumn) // move right
                {
                    foreach(Position pos in ActiveBlock.PossiblePositions[GetRotationIndex(CW)])
                    {
                        int i = pos.Row + ActiveBlock.Offset.Row;
                        int j = pos.Column + ActiveBlock.Offset.Column;
                        if (j+collisions>9 || OccupiedCells[i][j+collisions].Item1)
                            return;
                    }
                    SavePreviousPosition();
                    ActiveBlock.Offset.Column += collisions;
                    SavePreviousPosition();
                    ActiveBlock.CurrentPosition = GetRotationIndex(CW);
                }
                else // move left
                {
                    foreach (Position pos in ActiveBlock.PossiblePositions[GetRotationIndex(CW)])
                    {
                        int i = pos.Row + ActiveBlock.Offset.Row;
                        int j = pos.Column + ActiveBlock.Offset.Column;
                        if (j-collisions<0 || OccupiedCells[i][j - collisions].Item1)
                            return;
                    }
                    SavePreviousPosition();
                    ActiveBlock.Offset.Column -= collisions;
                    SavePreviousPosition();
                    ActiveBlock.CurrentPosition = GetRotationIndex(CW);
                }

            }
        }

        private void HoldBlock()
        {
            Models.Block newBlock = null;
            if (HeldBlock == null)
                newBlock = blockQueue.GenerateBlock();
            else
                newBlock = HeldBlock;
            SavePreviousPosition();
            newBlock.PreviousPosition = ActiveBlock.PreviousPosition;
            HeldBlock = ActiveBlock;
            ActiveBlock = newBlock;
            HeldBlock.ResetPositions();
        }

        private void PlaceBlock()
        {
            foreach(Position position in ActiveBlock.PossiblePositions[ActiveBlock.CurrentPosition])
            {
                
                OccupiedCells[position.Row + ActiveBlock.Offset.Row][position.Column + ActiveBlock.Offset.Column] = (true,ActiveBlock.Color.Color);
            }
            ActiveBlock = blockQueue.GenerateBlock();
            CheckClearRows();
        }

        private void ClearRow(int row)
        {
            ObservableCollection<ObservableCollection<(bool, Color)>> Occupado = new ObservableCollection<ObservableCollection<(bool, Color)>>();
            for (int i=row; i>0; i--)
            {
                ObservableCollection<(bool,Color)> newRow = new ObservableCollection<(bool,Color)> ();
                
                for (int column = 0; column < 10; column++)
                {
                    newRow.Add(OccupiedCells[i - 1][column]);  
                }
                Occupado.Add(newRow);
            }
            ObservableCollection<(bool, Color)> EmptyRow = new ObservableCollection<(bool, Color)>();
            for(int column = 0; column < 10; column++)
            {
                EmptyRow.Add((false, Colors.Transparent));
            }
            Occupado.Add(EmptyRow);
            Occupado = new ObservableCollection<ObservableCollection<(bool, Color)>>(Occupado.Reverse());
            for(int i= row+1; i<22; i++)
            {
                Occupado.Add(OccupiedCells[i]);
            }
            OccupiedCells = Occupado;
        }

        private async void CheckClearRows()
        {
            for(int row = 0; row < 22; row++)
            {
                bool isFull = true;

                for (int column = 0; column < 10; column++)
                {
                    if (!OccupiedCells[row][column].Item1)
                    {
                        isFull = false;
                        break;         
                    }
                }

                if (isFull)
                {
                    ClearRow(row);
                }
            }
        }
        private async void StartGame()
        {
            isRunning = true;
            ActiveBlock = blockQueue.CurrentBlock;
            while (isRunning)
            {
                await Task.Delay(400);
                MoveDown();
            }
        }

        private void InitiliazeOccupiedCells()
        {
            OccupiedCells = new ObservableCollection<ObservableCollection<(bool, Color)>>();

            for (int row = 0; row < 22; row++)
            {
                var rowCollection = new ObservableCollection<(bool, Color)>();
                for (int col = 0; col < 10; col++)
                {
                    rowCollection.Add((false, Colors.Transparent));
                }
                OccupiedCells.Add(rowCollection);
            }
        }

        public MainViewModel()
        {
            InitiliazeOccupiedCells();

        }
    }
}
