using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
// 
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DesignAutomationFramework;

namespace RevitConversion
{
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    
    public class WallObject {
        public int level = 0;
        public List<Coordinate> coordinates;        
        public WallObject(int _level, List<Coordinate> _coordinates) {
            level = _level;
            coordinates = _coordinates;
        }              
    }
    
    public class Coordinate {
        public decimal x = 0;
        public decimal y = 0;
        public Coordinate(decimal _x, decimal _y) {
            x = _x;
            y = _y;
        }
        public string logDisplay() {
            return x + ", " + y;
        }
    }
    
    public class LevelObject {
        public decimal elevation = 0;
        public string category;
        public bool has_category = false;
        public LevelObject(decimal _elevation, string _category, bool _has_category) {
            elevation = _elevation;
            has_category = _has_category;
            if (_has_category) {
                category = _category;
            }
        }
    }
    
    public class BuildingObject {
        public List<WallObject> walls;
        public List<LevelObject> levels;
        public BuildingObject(
            List<WallObject> _walls, 
            List<LevelObject> _levels
        ) {
            levels = _levels;
            walls = _walls;
        }  
        
        public void logDetails() {
            Console.WriteLine("Walls: ");
            int wallIndex = 0;
            foreach (var _wall in walls) {
                Console.WriteLine("   Wall index: " + wallIndex);
                Console.WriteLine("      Level: " + _wall.level);
                Console.WriteLine("      Coordinates: " + _wall.coordinates.Count);
                foreach (var _coord in _wall.coordinates) {
                    Console.WriteLine("         " + _coord.x + ", " + _coord.y);                   
                }
                wallIndex++;
            }
            Console.WriteLine("Levels: ");
            int levelIndex = 0;
            foreach (var _level in levels) {
                Console.WriteLine("   Level index: " + levelIndex);
                Console.WriteLine("      Elevation: " + _level.elevation);
                Console.WriteLine("      Category: " + _level.category);
                Console.WriteLine("      has_category: " + _level.has_category);
                levelIndex++;
            }
        }    

        public void createRVTWalls(
            Document targetDoc, 
            // *** REQUIRES REVIT:
            List<Curve> curves_list, 
            int levelIndex
        ) {
            foreach (Curve _curve in curves_list) {                
                // *** REQUIRES REVIT:
                Wall.Create(targetDoc, _curve, levelIndex, false);
            }
        }
        public void createRVTFile(DesignAutomationData data) {
            // *** REQUIRES REVIT:
            if (data == null) { throw new InvalidDataException(nameof(data)); }
            Application rvtApp = data.RevitApp;
            if (rvtApp == null) { throw new InvalidDataException(nameof(rvtApp)); }
            Document newDoc = rvtApp.NewProjectDocument(UnitSystem.Imperial);
            if (newDoc == null) { throw new InvalidOperationException("Could not create new document."); }
            // 
            string filePath = "sketchIt.rvt";
            // *** LOOP THROUGH BUILDING WALLS ***
            int wallIndex = 0;
            foreach (WallObject _wall in walls) {
                Console.WriteLine("wallIndex: " + wallIndex);
                int coordIndex = 0;
                List<Coordinate> wall_coords = _wall.coordinates;
                int coordCount = wall_coords.Count;
                int maxCoordIndex = coordCount - 1;
                // *** DEFINE LIST OF CURVES FOR EACH WALL ***
                List<Curve> wall_curves = new List<Curve>();
                int levelID = _wall.level;
                //
                foreach (Coordinate wall_coord in wall_coords) {
                    Coordinate startCoord = new Coordinate(0, 0);
                    Coordinate endCoord = new Coordinate(0, 0);                        
                    if (coordIndex < maxCoordIndex) {
                        startCoord = wall_coords[coordIndex];
                        endCoord = wall_coords[coordIndex + 1];
                    }
                    else if (coordIndex == maxCoordIndex && coordCount > 1) {
                        startCoord = wall_coords[maxCoordIndex];
                        endCoord = wall_coords[0];                        
                    }
                    Console.WriteLine("      startCoord: " + startCoord.logDisplay());
                    Console.WriteLine("      endCoord: " + endCoord.logDisplay());
                    Console.WriteLine("      ");
                    coordIndex ++;
                    // *** CREATE START AND END XYZ OBJECT ***
                    // *** REQUIRES REVIT:
                    XYZ start = new XYZ(startCoord.x, startCoord.y, 0.0);
                    XYZ end = new XYZ(endCoord.x, endCoord.y, 0.0);
                    // *** ADD TO WALL CURVES LIST ***
                    wall_curves.Add(Line.CreateBound(start, end));
                }
                wallIndex ++;
                // *** CREATE WALLS USING CURVES ***
                // *** REQUIRES REVIT:
                using (Transaction wallTrans = new Transaction(newDoc, "Create some walls"))
                {
                    wallTrans.Start();
                    this.createRVTWalls(newDoc, wall_curves, levelID);
                    wallTrans.Commit();
                }                    
            }            
            // *** LOOP THROUGH BUILDING LEVELS ***
            int levelIndex = 0;
            foreach (LevelObject _level in levels) {
                decimal _elevation = _level.elevation;
                Console.WriteLine("_elevation: " + _elevation);
                // *** REQUIRES REVIT:
                Level revitLevel = Level.Create(newDoc, _level.elevation);
                if (revitLevel == null) {
                    throw new Exception("Create a new level failed.");
                }
                revitLevel.Name = "Level " + levelIndex;
                if (_level.has_category) {
                    string _category = _level.category;
                    Console.WriteLine("_category: " + _category);
                    revitLevel.Category = _level.category;
                }                
                levelIndex++;
            }
             newDoc.SaveAs(filePath);
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            //Your code goes here
            Console.WriteLine("Hello, world!");
            //
            Coordinate x1 = new Coordinate(1, 0);
            Coordinate x2 = new Coordinate(0, 1);
            Coordinate x3 = new Coordinate(0, 0);
            //
            LevelObject level_1 = new LevelObject(10, "", false);
            LevelObject level_2 = new LevelObject(20, "", false);
            LevelObject level_3 = new LevelObject(30, "", false);
            LevelObject level_4 = new LevelObject(40, "Lobby", true);
            //
            List<Coordinate> building_xy_coords = new List<Coordinate>{x1, x2, x3};
            WallObject wall_1 = new WallObject(0, building_xy_coords);
            WallObject wall_2 = new WallObject(1, building_xy_coords);
            WallObject wall_3 = new WallObject(2, building_xy_coords);
            WallObject wall_4 = new WallObject(3, building_xy_coords);
            //
            List<WallObject> sample_building_walls = new List<WallObject>{wall_1, wall_2, wall_3, wall_4};
            List<LevelObject> sample_building_levels = new List<LevelObject>{level_1, level_2, level_3, level_4};
            //
            BuildingObject sample_building = new BuildingObject(sample_building_walls, sample_building_levels);
            //sample_building.logDetails();
            sample_building.createRVTFile();
            //
            // sample_building looks like:
            //  {
            //   walls: [
            //     {
            //       coordinates: [
            //         { x: 1, y: 0 },
            //         { x: 0, y: 1 },
            //         { x: 0, y: 0 }
            //       ],
            //       level: 0
            //     },
            //     {
            //       coordinates: [
            //         { x: 1, y: 0 },
            //         { x: 0, y: 1 },
            //         { x: 0, y: 0 }
            //       ],
            //       level: 1
            //     },
            //     {
            //       coordinates: [
            //         { x: 1, y: 0 },
            //         { x: 0, y: 1 },
            //         { x: 0, y: 0 }
            //       ],
            //       level: 2
            //     },
            //     {
            //       coordinates: [
            //         { x: 1, y: 0 },
            //         { x: 0, y: 1 },
            //         { x: 0, y: 0 }
            //       ],
            //       level: 3
            //     }
            //   ],
            //   levels: [
            //     {
            //       elevation: 10,
            //       category: null,
            //       has_category: false
            //     },
            //     {
            //       elevation: 20,
            //       category: null,
            //       has_category: false
            //     },
            //     {
            //       elevation: 30,
            //       category: null,
            //       has_category: false
            //     },
            //     {
            //       elevation: 40,
            //       category: "Lobby",
            //       has_category: true
            //     },
            //   ]
            // }
        }
    }
}