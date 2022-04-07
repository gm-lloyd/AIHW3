using System;
using System.Collections.Generic;

/// <summary>
/// program to perform minimax algorithm on three men's morris game
/// author: Gavin Lloyd
/// 
/// Whoever starts will win
/// 
/// Avg time for Max first -> 8.7 secs
/// Avg time for Min first -> 8.4 secs
/// 
/// problem is represented by an array of size 9
/// this is translated to a 3x3 board by using the following indices in these positions
/// 012
/// 345
/// 678
/// so the array 
/// ["w"," "," ","b","w"," "," "," ","b"]
/// represents the board
/// _____
/// |w  |
/// |bw |
/// |  b|
/// -----
/// </summary>
namespace ThreeMensMorris
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Ma(x) or Mi(n) first? ");
			var input = Console.ReadLine();
			DateTime before = new(), after = new();
			Node root, tree;
			if (input == "x")
			{
				root = new(true);
				before = DateTime.Now;
				tree = AlphaBetaPruning(root, int.MinValue, int.MaxValue, true);
				after = DateTime.Now;

			}
			else if (input == "n")
			{
				root = new(false);
				before = DateTime.Now;
				tree = AlphaBetaPruning(root, int.MinValue, int.MaxValue, false);
				after = DateTime.Now;
			}
			else
			{
				tree = new(false);
				Console.WriteLine("invalid input");
				return;
			}
			//set breakpoint here to explore the tree in debugger if you want
			tree = tree;
			var timeTaken = after - before;

			Console.WriteLine(timeTaken.ToString());
			Traverse(tree, input == "x");
			
		}

		/// <summary>
		/// Allows the user to look at the moves that the ai explored.
		/// AI Move is printed and then user can pick between the nodes the ai explored
		/// some moves are not available because they were pruned, 
		/// ideal ai would not have picked the node if it doesn't have all available moves
		/// </summary>
		/// <param name="root">root of the tree to traverse</param>
		/// <param name="MaxIsCPU">determines whether max or min is cpu</param>
		public static void Traverse(Node root, bool MaxIsCPU)
		{
			Console.WriteLine(" final value is " + root.NodeValue);

			foreach (Node CPUMove in root.childrenNodes)
			{
				if (MaxIsCPU)
				{
					if (CPUMove.NodeValue == 8)
					{
						root = CPUMove;
						break;
					}
					
				}
				else
				{
					if (CPUMove.NodeValue == -8)
					{
						root = CPUMove;
						break;
					}
				}
			}

			while (!root.CheckWins())
			{
				Console.WriteLine("CPU's Move:");
				PrintBoard(root.state);

				var moveNum = 1;
				Console.WriteLine("Which Move do you pick?");
				foreach(Node playerMove in root.childrenNodes)
				{
					Console.WriteLine(moveNum);
					PrintBoard(playerMove.state);
					moveNum++;
				}
				int choice = int.Parse(Console.ReadLine());
				root = root.childrenNodes[choice - 1];

				foreach (Node CPUMove in root.childrenNodes)
				{
					if(MaxIsCPU)
					{
						if (CPUMove.NodeValue == 8)
							root = CPUMove;
					}
					else
					{
						if (CPUMove.NodeValue == -8)
							root = CPUMove;
					}
				}
			}
			Console.WriteLine("CPU's Move:");
			PrintBoard(root.state);
		}

		/// <summary>
		/// prints the state of the board to the console
		/// </summary>
		/// <param name="board">state of board</param>
		public static void PrintBoard(List<string> board)
		{
			Console.Write("---\n|" + board[0] + board[1] + board[2] + "|\n|"
						+ board[3] + board[4] + board[5] + "|\n|" 
						+ board[6] + board[7] + board[8] + "|\n---\n");
		}

		/// <summary>
		/// Recursive method to perform alpha beta pruning
		/// Max and min nodes take turns to select a move that will result in the highest or lowest score respectively
		/// does not continue if nodes searched do not lead to a better move for Max or Min
		/// </summary>
		/// <param name="node"> The current node to explore </param>
		/// <param name="alpha"> the highest value node that Max can guarantee</param>
		/// <param name="beta"> the lowest value node that Min can guarantee</param>
		/// <param name="MaxTurn"> bool to control which turn it is</param>
		/// <returns></returns>
		public static Node AlphaBetaPruning(Node node,int alpha, int beta, bool MaxTurn)
		{
			if (node.CheckWins() || node.depth >= 16)
				return node;
			else
			{
				if (MaxTurn)
				{
					// list of all possible moves, not everyone is guaranteed to be explored
					List<Node> childNodes = node.GetAllChildren(MaxTurn);
					node.NodeValue = int.MinValue;
					foreach (Node n in childNodes)
					{
						Node child = AlphaBetaPruning(n, alpha, beta, !MaxTurn);
						//if explored, add to the tree
						node.childrenNodes.Add(child);
						child.parentNode = node;
						//set max value
						node.NodeValue = Math.Max(node.NodeValue, child.NodeValue);
						//set alpha
						alpha = Math.Max(alpha, node.NodeValue);
						if (alpha >= beta)
						{
							break;
							//cutoff
						}

					}

				}
				else //MinTurn, same process but reversed for Min 
				{
					List<Node> childNodes = node.GetAllChildren(MaxTurn);
					node.NodeValue = int.MaxValue;
					foreach (Node n in childNodes)
					{
						Node child = AlphaBetaPruning(n, alpha, beta, !MaxTurn);
						node.childrenNodes.Add(child);
						child.parentNode = node;
						node.NodeValue = Math.Min(node.NodeValue, child.NodeValue);

						beta = Math.Min(beta, node.NodeValue);

						if (alpha >= beta)
						{
							break;
							//cutoff
						}

					}
				}
			}

			//return the node if it isn't a leaf to have intermediates
			return node;
		}
	}

	/// <summary>
	/// Node class for representing the search tree
	/// </summary>
	class Node
	{
		/// <summary>
		/// List of 9 holding game state, -1 is white, 1 is black 0 is empty
		/// 012
		/// 345
		/// 678
		/// the index holds the 
		/// </summary>
		public List<string> state;
		//value of node
		public int NodeValue = 0;
		//reference to rest of child nodes
		public List<Node> childrenNodes;
		//keep track of parent nodes
		public Node parentNode;
		//depth needed for ply
		public int depth = 0;
		// used for calcs for Max or Min
		public bool MaxTurn;

		public Node(bool MaxFirst, Node parent = null)
		{
			state = new List<string> { " ", " ", " ", " ", " ", " ", " ", " ", " " };
			MaxTurn = MaxFirst;
			parentNode = parent;
			childrenNodes = new();
		}


		public Node(List<string> currentState, bool MaxTurn, Node parent, int depth)
		{
			state = currentState;
			this.MaxTurn = MaxTurn;
			parentNode = parent;
			this.depth = depth;
			childrenNodes = new();
		}

		/// <summary>
		/// goes through the board state and checks each possible winning path for a win, if a win is found, returns true.
		/// If no wins are found, the difference in potential winning paths for black and white are computed and set and returns false
		/// </summary>
		/// <returns>whether the board contains a win</returns>
		public bool CheckWins()
		{
			//top row
			string line = state[0] + state[1] + state[2];
			bool win = CheckLine(line);
			if (win)
				return true;
			//middle row
			line = state[3] + state[4] + state[5];
			win = CheckLine(line);
			if (win)
				return true;
			//bottom row
			line = state[6] + state[7] + state[8];
			win = CheckLine(line);
			if (win)
				return true;
			//left column
			line = state[0] + state[3] + state[6];
			win = CheckLine(line);
			if (win)
				return true;
			//middle column
			line = state[1] + state[4] + state[7];
			win = CheckLine(line);
			if (win)
				return true;
			//right column
			line = state[2] + state[5] + state[8];
			win = CheckLine(line);
			if (win)
				return true;
			//top left to bottom right diagonal
			line = state[0] + state[4] + state[8];
			win = CheckLine(line);
			if (win)
				return true;
			//bottom left to top right diagonal
			line = state[6] + state[4] + state[2];
			win = CheckLine(line);
			if (win)
				return true;

			return false;
		}

		/// <summary>
		/// Given a string of three spaces on the board in a line, checks if it is a win and returns 8 or -8
		/// If no win is found, but it is a winning path for 
		/// </summary>
		/// <param name="line">string of three consecutive spaces on the board</param>
		/// <returns>the line is a win or not</returns>
		private bool CheckLine(string line)
		{
			if (line == "www")
			{
				NodeValue = 8;
				return true; //white wins
			}
			else if (line == "bbb")
			{
				NodeValue = -8;
				return true; //black wins
			}
			else if (line.Contains("b") && !line.Contains("w"))
			{
				NodeValue += -1;
				return false; //has black pieces and no white pieces, one more winning path for black
			}
			else if (line.Contains("w") && !line.Contains("b"))
			{
				NodeValue += 1;
				return false; //one more winning path for white

			}
			else return false;
		}

		/// <summary>
		/// finds all possible moves for the current board and returns a list of nodes with those moves
		/// </summary>
		/// <param name="whiteMove">if it is white (Max) to move</param>
		/// <returns></returns>
		public List<Node> GetAllChildren(bool whiteMove)
		{
			List<Node> possibleMoves = new();

			string piece = whiteMove ? "w" : "b";
			// if player has not placed three yet
			if (state.FindAll(delegate (string s) { return s == piece; }).Count < 3)
			{
				int i = 0;
				foreach (string value in state)
				{
					if (value == " ")
					{
						//place the piece in the board
						List<string> newState = new(state);
						newState[i] = piece;
						//add this move to the possible moves
						possibleMoves.Add(new Node(newState, !whiteMove, this, depth + 1));
					}
					i++;
				}
			}
			else // player has placed all pieces and now moves already placed pieces
			{
				int i = 0;
				foreach (string value in state)
				{
					if (value == piece)
					{
						List<int> possibleIndex = getPossibleMovesFor(state, i);
						//for each adjacent empty space, add node with that move to the possible moves
						foreach (int n in possibleIndex) { possibleMoves.Add(new Node(swap(state, i, n), !whiteMove, this, depth + 1)); }
					}
					i++;
				}
			}

			//return all possible moves as children for the node
			return possibleMoves;

		}

		/// <summary>
		/// takes in the state of the game and the index of a piece and gets the possible adjacent indeices that are empty
		/// </summary>
		/// <param name="state">position of pieces</param>
		/// <param name="i">position of piece to move</param>
		/// <returns>list of empty adjacent indices</returns>
		private List<int> getPossibleMovesFor(List<string> state, int i)
		{
			List<int> possibleIndex = new();
			if (i != 4 && state[4] == " ")
			{
				//move to the middle if its open
				possibleIndex.Add(4);
			}

			if (i == 0)
			{
				if (state[1] == " ") { possibleIndex.Add(1); }
				if (state[3] == " ") { possibleIndex.Add(3); }
			}
			if (i == 1)
			{
				if (state[0] == " ") { possibleIndex.Add(0); }
				if (state[2] == " ") { possibleIndex.Add(2); }
			}
			if (i == 2)
			{
				if (state[1] == " ") { possibleIndex.Add(1); }
				if (state[5] == " ") { possibleIndex.Add(5); }
			}
			if (i == 3)
			{
				if (state[0] == " ") { possibleIndex.Add(0); }
				if (state[6] == " ") { possibleIndex.Add(6); }
			}
			if (i == 5)
			{
				if (state[2] == " ") { possibleIndex.Add(2); }
				if (state[8] == " ") { possibleIndex.Add(8); }
			}
			if (i == 4)
			{
				for (int n = 0; n < 9; n++)
				{
					if (n != 4) { if (state[n] == " ") { possibleIndex.Add(n); } }

				}

			}
			if (i == 6)
			{
				if (state[3] == " ") { possibleIndex.Add(3); }
				if (state[7] == " ") { possibleIndex.Add(7); }
			}
			if (i == 7)
			{
				if (state[6] == " ") { possibleIndex.Add(6); }
				if (state[8] == " ") { possibleIndex.Add(8); }
			}
			if (i == 8)
			{
				if (state[7] == " ") { possibleIndex.Add(7); }
				if (state[5] == " ") { possibleIndex.Add(5); }
			}



			return possibleIndex;
		}

		/// <summary>
		/// swaps the two indices given and returns a new board state 
		/// </summary>
		/// <param name="oldState">state to swap from</param>
		/// <param name="blankIndex">one index to swap</param>
		/// <param name="pieceIndex">other index to swap</param>
		/// <returns>state with swap</returns>
		private static List<string> swap(List<string> oldState, int blankIndex, int pieceIndex)
		{
			List<string> newState = new(oldState);
			newState[blankIndex] = oldState[pieceIndex];
			newState[pieceIndex] = oldState[blankIndex];
			return newState;
		}
	}
}
