using SeaFightDll.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeaFightDll.Core
{
    public class GameField
    {
        private StringBuilder stringBuilder = new StringBuilder();
        private Tuple<Point, BaseShip>[,] gameField;
        private int sizeField;

        public GameField(int sizeField)
        {
            if (sizeField % 2 != 0) throw new Exception("Размер поля должен быть кратным нулю");

            this.sizeField = sizeField;

            this.InitializeGameField();
        }

        private void InitializeGameField()
        {
            this.gameField = new Tuple<Point, BaseShip>[this.sizeField, this.sizeField];

            var middleField = this.sizeField / 2;
            var startPoinX = this.sizeField / 2 - 1;
            var startPointY = startPoinX;

            var iThirdQuadrant = this.sizeField - 1;
            var jFirstQuadrant = this.sizeField - 1;
            var iFourthQuadrant = this.sizeField - 1;
            var jFourthQuadrant = this.sizeField - 1;

            for (int i = 0; i < middleField; i++)
            {
                for (int j = 0; j < middleField; j++)
                {
                    var point = new Point(x: startPoinX, y: startPointY);
                    var tuple = new Tuple<Point, BaseShip>(point, null);

                    // 1-й квадрант - 2-й квадрант - 3-й квадрант - 4-й квадрант
                    gameField[i, jFirstQuadrant] = gameField[i, j] =  gameField[iThirdQuadrant - i, j] = gameField[iFourthQuadrant, jFourthQuadrant] = tuple;

                    jFirstQuadrant--;
                    jFourthQuadrant--;
                    startPointY--;
                }

                startPoinX--;
                iFourthQuadrant--;
                startPointY = (sizeField / 2) - 1;
                jFourthQuadrant = jFirstQuadrant = sizeField - 1;
            }
        }
        
        public void AddShip(BaseShip ship, Point point, Position position, Quadrant quadrant)
        {
            var coordinates = this.CoordinatesOfPoint(this.gameField, point, quadrant);

            if (this.gameField[coordinates.x, coordinates.y].Item2 is null)
            {
                var (startPoint, endPoint) = this.GetPoints(position, coordinates.x, coordinates.y, ship.Length);

                while (startPoint != endPoint)
                {
                    switch (position)
                    {
                        case Position.Vertical:
                            {
                                if (!(this.gameField[startPoint, coordinates.y].Item2 is null))
                                    throw new Exception("Этот корабль не влезает по горизонтали!");
                                break;
                            }
                        case Position.Horizontal:
                            {
                                if (!(this.gameField[coordinates.x, startPoint].Item2 is null))
                                    throw new Exception("Этот корабль не влезает по вертикали!");
                                break;
                            }
                    }
                    startPoint++;
                }

                (startPoint, endPoint) = this.GetPoints(position, coordinates.x, coordinates.y, ship.Length);

                while (startPoint != endPoint)
                {
                    switch (position)
                    {
                        case Position.Vertical:
                            gameField[startPoint, coordinates.y] = Tuple.Create(gameField[startPoint, coordinates.y].Item1, ship);
                            break;
                        case Position.Horizontal:
                            gameField[coordinates.x, startPoint] = Tuple.Create(gameField[coordinates.x, startPoint].Item1, ship);
                            break;
                    }
                    startPoint++;
                }
            }
            else
            {
                throw new Exception("Данная точка размещения занята!");
            }
        }

        private (int x, int y) CoordinatesOfPoint(Tuple<Point, BaseShip>[,] matrix, Point point, Quadrant quadrant)
        {
            int i = 0, j = 0;
            int iLast = 0, jLast = 0;

            switch (quadrant)
            {
                case Quadrant.First:
                    i = 0;
                    jLast = this.sizeField;
                    j = iLast = this.sizeField / 2;
                    break;
                case Quadrant.Second:
                    i = j = 0;
                    iLast = jLast = this.sizeField / 2;
                    break;
                case Quadrant.Third:
                    j = 0;
                    iLast = this.sizeField;
                    i = jLast = this.sizeField / 2;
                    break;
                case Quadrant.Fourth:
                    i = j = this.sizeField / 2;
                    iLast = jLast = this.sizeField;
                    break;
            }

            for (int x = i; x < iLast; x++)
            {
                for (int y = j; y < jLast; y++)
                {
                    if (matrix[x, y].Item1.X == point.X && matrix[x, y].Item1.Y == point.Y)
                        return (x, y);
                }
            }

            return (-1, -1);
        }

        private (int startPoint, int endPoint) GetPoints(Position position, int coordinatesX, int coordinatesY, int shipLength)
        {
            int startPoint = position == Position.Horizontal ? coordinatesY : coordinatesX;
            int endPoint = startPoint + shipLength;

            return (startPoint, endPoint);
        }

        public BaseShip this[Quadrant quadrant, int pointX, int pointY]
        {
            get
            {
                var (x, y) = this.CoordinatesOfPoint(this.gameField, new Point(pointX, pointY), quadrant);

                if(x == -1 && y == -1)
                    throw new IndexOutOfRangeException("Не правильный индекс обращения!");
                else
                    return this.gameField[x, y].Item2;
            }
        }

        public string GetGameField()
        {
            stringBuilder.Clear();

            for (int i = 0; i < sizeField; i++)
            {
                for (int j = 0; j < sizeField; j++)
                {
                    stringBuilder.Append("[");
                    stringBuilder.Append(gameField[i, j]?.Item1.X);
                    stringBuilder.Append(",");
                    stringBuilder.Append(gameField[i, j]?.Item1.Y);
                    stringBuilder.Append(",");
                    stringBuilder.Append(gameField[i, j].Item2 is null ? "-" : "*");
                    stringBuilder.Append("]");
                }
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public IEnumerable<Tuple<Point, BaseShip>> SortByCenterField()
        {
            var result = this.gameField
                    .OfType<Tuple<Point, BaseShip>>()
                    .Where(x => x.Item2 is object)
                    .OrderBy(x => x.Item1.X)
                    .ThenBy(x => x.Item1.Y)
                    .DistinctBy(x => x.Item2)
                    .ToList();

            return result;
        }
    }
}