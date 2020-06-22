using System;
using System.Collections.Generic;
using SeaFightDll.Core;

namespace SeaFightApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameField = new GameField(sizeField: 10);

            var collectionShip = new BaseShip [3]
            {
                new WarShip(length: 4, speed: 2, rangeOfAction: 2, way: "На юг"),
                new WarShip(length: 3, speed: 2, rangeOfAction: 2, way: "На запад"),
                new AuxiliaryShip(length: 1, speed: 2, rangeOfAction: 2, way: "На восток")
            };

            var collectionPoints = new List<Point>
            {
                new Point(1, 2),
                new Point(0, 0),
                new Point(2, 2)
            };

            try
            {
                for (int i = 0; i < collectionShip.Length; i++)
                {
                    var position = i+1 % 2 == 0 ? Position.Horizontal : Position.Vertical;

                    gameField.AddShip(
                        ship: collectionShip[i],
                        point: collectionPoints[i],
                        position: position,
                        quadrant: (Quadrant)i
                        );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine($"\tКорабль по координатам [1, 2], квадрант Первый:\n{gameField[Quadrant.First, 1, 2]?.GetInfo()}\n");
            Console.WriteLine($"Корабль 1 = Корбль 2: {collectionShip[0] == collectionShip[1]}");
            Console.WriteLine($"Корабль 1 != Корбль 2: {collectionShip[0] != collectionShip[1]}");
            Console.WriteLine($"\n\tИговое поле:\n{gameField.GetGameField()}");
            Console.WriteLine("\tСписок кораблей на поле:\n");
            
            var collection = gameField.SortByCenterField();
            
            foreach (var item in collection)
            {
                Console.WriteLine($"{item.Item2.GetInfo()}\nТочка расположения: [{item.Item1.X},{item.Item1.Y}]\n");
            }

            Console.ReadLine();
        }
    }
}