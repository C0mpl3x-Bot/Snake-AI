using UnityEngine;
using AlanZucconi.AI.BT;
using AlanZucconi.Snake;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu
(
    fileName = "SnakeAI_oguve001",
    menuName = "Snake/2021-22/SnakeAI_oguve001"
)]
public class SnakeAI_oguve001 : SnakeAI
{

    //intialise variables
    int x;
    int y;

    int k;
    int l;

    int max;


    public override Node CreateBehaviourTree(SnakeGame Snake)
    {
        //checks if food is reachable but tail is not
        bool FoodReachableButNotTail()
        {
            return !Snake
                .AvailableNeighbours(Snake.TailPosition)
                .Any(position => Snake.IsReachable(position))
                && Snake.IsFoodReachable();
        }
        //checks if tail and food are reachable
        bool AreTailAndFoodReachable()
        {
            return Snake
                .AvailableNeighbours(Snake.TailPosition)
                .Any(position => Snake.IsReachable(position))
                && Snake.IsFoodReachable();
        }
        //checks if tail is only reachable and food is not 
        bool IsTailOnlyReachable()
        {
            return Snake
                .AvailableNeighbours(Snake.TailPosition)
                .Any(position => Snake.IsReachable(position))
                && !Snake.IsFoodReachable();
        }
        
        //checks if food and tail are not reachable
        bool FoodNotReachableAndTail()
        {
            return (!Snake
                .AvailableNeighbours(Snake.TailPosition)
                .Any(position => Snake.IsReachable(position)))
                && (!Snake.IsFoodReachable());
        }

        //checks if food is safe by comparing number of obstacles around food and if it is greater than or equal to 2 then it returns false
        bool FoodSafe()
        {
            int unReachable = 0;
            foreach (var p in Snake.AvailableNeighbours(Snake.FoodPosition))
            {
                if (Snake.IsObstacle(p))
                {
                    unReachable++;
                }
            }
            return unReachable < 2;
        }

        //checks if head is safe by comparing number of obstacles around head and if it is greater than 2 then it returns false
        bool HeadSafe()
        {
            int unReachable = 0;
            foreach (var p in Snake.AvailableNeighbours(Snake.HeadPosition))
            {
                if (Snake.IsObstacle(p))
                {
                    unReachable++;
                }
            }
            return unReachable <= 1;
        }



        return
            //selector initialised
            new Selector(
                new Filter(
                    //condition checks if snake size less than or equal to 50 and if food is reachable and safe
                    new Condition(() => Snake.Body.Count <= 50 && Snake.IsFoodReachable() && FoodSafe()),
                    //if condition is met then the snake moves towards food
                    new Action(Snake.MoveTowardsFood)
                ),
                new Filter(
                    //condition checks if:
                    //tail is reachable but food is not or
                    //Obstacle is ahead and food and tail are reachable and tail and head have a distance greater than 3 or
                    //food is not safe and tail and food are reachable and tail and head have a distacne greater than 3 
                    //random range to make condition false so it stops infinte loops.
                    new Condition(() => ((IsTailOnlyReachable() ||
                    Snake.IsObstacleAhead() && AreTailAndFoodReachable() && Vector2Int.Distance(Snake.TailPosition, Snake.HeadPosition) > 3) ||
                    !FoodSafe() && AreTailAndFoodReachable() && Vector2Int.Distance(Snake.TailPosition, Snake.HeadPosition) > 3) && Random.Range(0f, 1f) < 0.9),

                    //if any of the conditions in the condition is met then the snake would move towards its tail
                    new Action(() => Snake.MoveTowards(
                        Snake
                        .AvailableNeighbours(Snake.TailPosition)
                        .FirstOrDefault(position => Snake.IsReachable(position)))
                    )
                ),
                
                new Filter(
                    //condtion checks if tail and food are reachable and snake size is greater than 50 and food is in a safe position
                    new Condition(() => AreTailAndFoodReachable() && Snake.Body.Count > 50 && FoodSafe()),
                    //if condition met then the snake would move towards the food
                    new Action(Snake.MoveTowardsFood)
                ),

                new Filter(
                    //condition checks if snake head is not safe
                    new Condition(() => !HeadSafe()),
                    new Action(() =>
                    {
                        //finds distance of the closest obstacle to the left of the snake from its head position and saves it to HeadToLeft
                        int HeadToLeft = Snake.DistanceFrom(Snake.HeadPosition, Snake.RaycastLeft().Position);
                        // finds distance of the closest obstacle to the right of the snake from its head position and saves it to HeadToRight
                        int HeadToRight = Snake.DistanceFrom(Snake.HeadPosition, Snake.RaycastRight().Position);

                        //checks if left obstacle is further away from right obstacle and checks if the snake can reach the left obstacle
                        if (HeadToLeft > HeadToRight && Snake.IsReachable(Snake.RaycastLeft().Position))
                        {
                            //snake turns towards the left obstacle
                            Snake.TurnLeft();
                        }
                        //checks if right obstacle is further away from left obstacle and checks if the snake can reach the right obstacle
                        else if (HeadToLeft < HeadToRight && Snake.IsReachable(Snake.RaycastRight().Position))
                        {
                            //snake turns towards the right obstacle
                            Snake.TurnRight();
                        }

                    })
                ),

                //just live until tail is reachable
                new Filter(
                    //condition checks if 
                    //food is reachable and tail is not or 
                    //food and tail are not reachable and snake size greater than or equal to 60 and head is safe
                    new Condition(() => (FoodReachableButNotTail() || FoodNotReachableAndTail() && Snake.Body.Count >= 60) && HeadSafe()),
                    new Filter(
                        new Condition(() =>
                        {
                            //for loop goes from 0 to 25 (not including)
                            for (int i = 0; i < 25; i++)
                            {
                                //for loop goes from 0 to 25 (not including)
                                for (int j = 0; j < 25; j++)
                                {
                                    //checks if i,j is equal to the head position
                                    if (new Vector2Int(i, j) == Snake.HeadPosition)
                                    {
                                        continue;
                                    }
                                    //checks if i,j is an obstacle on the grid
                                    if (Snake.IsObstacle(new Vector2Int(i, j)))
                                    {
                                        continue;
                                    }
                                    //checks if snake can reach i,j
                                    if (Snake.IsReachable(new Vector2Int(i, j)))
                                    {
                                        //set k to i
                                        k = i;
                                        //set l to j
                                        l = j;
                                        return true;

                                    }
                                }
                            }
                            return false;

                        }),
                        new Action(() => {
                            //if condition returns true then snake moves to k,l
                            Snake.MoveTowards(k, l);
                        })


                    )
                ),

                new Filter(
                    new Condition(() =>
                    {
                        //checks if food and tail are not reachable and checks that the snake size is less than 60
                        if (FoodNotReachableAndTail() && Snake.Body.Count < 60)
                        {
                            //creates new list with type vector2Int
                            List<Vector2Int> list = new List<Vector2Int>();
                            //for loop from 0 to 25 (not including)
                            for (int i = 0; i < 25; i++)
                            {
                                //for loop from 0 to 25 (not including)
                                for (int j = 0; j < 25; j++)
                                {
                                    //checks if i,j is equal to the head position
                                    if (new Vector2Int(i, j) == Snake.HeadPosition)
                                    {
                                        continue;
                                    }
                                    //checks if i,j is an obstacle on the grid
                                    if (Snake.IsObstacle(new Vector2Int(i, j)))
                                    {
                                        continue;
                                    }
                                    //adds i,j to the list
                                    list.Add(new Vector2Int(i, j));
                                }
                            }

                            //shuffle the list
                            for (int i = 0; i < list.Count; i++)
                            {
                                //int j is a random point within the list
                                int j = Random.Range(0, list.Count);
                                //swaps elements in index i with elements in index j of the list 
                                Vector2Int temp = list[i];
                                list[i] = list[j];
                                list[j] = temp;

                            }
                            foreach (Vector2Int p in list)
                            {
                                //checks if snake can reach p
                                if (Snake.IsReachable(p))
                                {
                                    //sets x to p.x
                                    x = p.x;
                                    //sets y to p.y
                                    y = p.y;
                                    return true;
                                }
                            }
                        }
                        return false;
                    }),
                    
                    new Action(() =>
                    {
                        //if condition returns true then snake moves to x,y
                        Snake.MoveTowards(x, y);
                    })
                ),

                //action is none of the conditions are met
                new Action(() =>
                {
                    //gets distance of the head to the bottom left corner of the grid and saves it to HeadToBottomLeft
                    int HeadToBottomLeft = Snake.DistanceFrom(Snake.HeadPosition, new Vector2Int(1, 1));

                    //gets distance of the head to the bottom right corner of the grid and saves it to HeadToBottomRight
                    int HeadToBottomRight = Snake.DistanceFrom(Snake.HeadPosition, new Vector2Int(1, 24));

                    //gets distance of the head to the Top right corner of the grid and saves it to HeadToTopRight
                    int HeadToTopRight = Snake.DistanceFrom(Snake.HeadPosition, new Vector2Int(24, 24));

                    //gets distance of the head to the Top left corner of the grid and saves it to HeadToTopLeft
                    int HeadToTopLeft = Snake.DistanceFrom(Snake.HeadPosition, new Vector2Int(24, 1));

                    //checks if the snake can not reach the bottom left corner
                    if (!Snake.IsReachable(new Vector2Int(1, 1)))
                    {
                        //if snake can not reach bottom left it sets HeadToBottomLeft to 0;
                        HeadToBottomLeft = 0;
                    }
                    //checks if the snake can not reach the bottom right corner
                    else if (!Snake.IsReachable(new Vector2Int(1, 24)))
                    {
                        //if snake can not reach bottom left it sets HeadToBottomRight to 0;
                        HeadToBottomRight = 0;
                    }
                    //checks if the snake can not reach the top right corner
                    else if (!Snake.IsReachable(new Vector2Int(24, 24)))
                    {
                        //if snake can not reach top right it sets HeadToTopRight to 0;
                        HeadToTopRight = 0;
                    }
                    //checks if the snake can not reach the top Left corner
                    else if (!Snake.IsReachable(new Vector2Int(24, 1)))
                    {
                        //if snake can not reach top right it sets HeadToTopLeft to 0;
                        HeadToTopLeft = 0;
                    }

                    //sets max to be the distance of the far most corner from the head
                    if (max < HeadToBottomLeft)
                    {
                        max = HeadToBottomLeft;
                    }
                    if (max < HeadToBottomRight)
                    {
                        max = HeadToBottomRight;
                    }
                    if (max < HeadToTopRight)
                    {
                        max = HeadToTopRight;
                    }
                    if (max < HeadToTopLeft)
                    {
                        max = HeadToTopLeft;
                    }

                    var current = Snake.Body.Last;
                    while (current != null)
                    {
                        //b is the neightbours of current.value
                        foreach (var b in Snake.AvailableNeighbours(current.Value))
                        {
                            //checks if b is reachable and has Random.Range(0f, 1f) < 0.6 to stop infinte loop of snake following body
                            if (Snake.IsReachable(b) && Random.Range(0f, 1f) < 0.6)
                            {
                                //snake moves towards b
                                Snake.MoveTowards(b);
                                return;
                            }
                        }
                        current = current.Previous;

                        //checks which distance max is equal to and would move to that corner which is the far most reachable corner.
                        if (max == HeadToBottomLeft)
                        {
                            Snake.MoveTowards(new Vector2Int(1, 1));
                        }
                        else if (max == HeadToBottomRight)
                        {
                            Snake.MoveTowards(new Vector2Int(1, 24));
                        }
                        else if (max == HeadToTopRight)
                        {
                            Snake.MoveTowards(new Vector2Int(24, 24));
                        }
                        else if (max == HeadToTopLeft)
                        {
                            Snake.MoveTowards(new Vector2Int(24, 1));
                        }

                    }
                })


            );
    }
}