using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace WordFormations
{
    class EnglishDictionary
    {
		private HashSet<string> hs;
		private Task loadData;

		// This constructor will initiate the loading of all words located
		// in the given dictionary.
		public EnglishDictionary(string fileName)
		{
			hs = new HashSet<string>();
			loadData = LoadWords(fileName);

		}
		// Helper Task which asynschonrously loads the words into the hashset.
		private async Task LoadWords(string fileName)
		{
			var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;
			string namespaceName = "WordFormations";
			Stream stream = assembly.GetManifestResourceStream(namespaceName + "." + fileName);

			using (var dictReader = new System.IO.StreamReader(stream))
			{
				while (!dictReader.EndOfStream)
				{
					string line = await dictReader.ReadLineAsync().ConfigureAwait(false);
					hs.Add(line);
				}
				dictReader.Close();
			}
		}

		// This method will return the set of all words that can be formed using a
		// collection of Scrabble tiles, that has a minimum length.
		public ISet<string> ScrabbleWords(string tiles, int minLength)
		{
			if (!loadData.IsCompleted)
			{
				loadData.Wait();
			}
			HashSet<string> possibleWords = new HashSet<string>();
			foreach (string word in hs) // For each word in the dictionary
			{
				if (ValidWordLength(word.Length, tiles.Length, minLength)) // If word length is within range, consider it 
				{
					if (IsCreatable(TilesToList(tiles), word))
					{
						possibleWords.Add(word);
					}
				}
			}
			return possibleWords;
		}

		// Private helper to convert a string of 'tiles' into a char[]
		private List<char> TilesToList(string tiles)
		{
			List<char> result = new List<char>();
			foreach (var tile in tiles.ToCharArray())
			{
				result.Add(tile);
			}
			return result;
		}

		// Private helper method to ScrabbleWords which will
		// determine if a given string can be created 
		// with the letters available
		private Boolean IsCreatable(List<char> tiles, string word)
		{
			int hits = 0; // Will count the 'hits', which are how many letters in the word that are also in the char[]
			int i = 0;
			while (i < word.Length)
			{
				if (tiles.Contains(word[i]))
				{
					hits++;
					tiles.Remove(word[i]);
				}
				i++;
			}
			if (hits == word.Length) // If there were the same number of hits as the length of the word, it can be made
			{
				return true;
			}
			return false;
		}

		// Private helper method to ScrabbleWords which will determine
		// if a given string is in range
		private Boolean ValidWordLength(int wordLength, int numTiles, int minLength)
		{
			if (wordLength <= numTiles && wordLength >= minLength)
			{
				return true;
			}
			return false;
		}

	}
}
