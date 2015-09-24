﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Security;

namespace Reversi.Persistence
{
    /// <summary>
    /// The type of the Reversi file manager.
    /// </summary>
    public class ReversiFileDataAccess : IReversiDataAccess
    {

        #region Fields 

        private readonly Int32[] _acceptableGameTableSizeArray;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public ReversiFileDataAccess(Int32[] acceptableGameTableSizeArray)
        {
            _acceptableGameTableSizeArray = acceptableGameTableSizeArray;
        }

        #endregion

        #region Public methodes

        /// <summary>
        /// Loading file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <returns>All the data to recover the game state. The game table size, the put downs and the players game times.</returns>
        public async Task<ReversiGameDescriptiveData> Load(String path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path)) // opening file
                {
                    // Read the first line of the file, then split it bye one space.
                    String line = await reader.ReadLineAsync();
                    String[] numbers = line.Split(' ');

                    // Setup fields with the processed data. The ordering is set by the file format.
                    Int32 tableSize = Int32.Parse(numbers[0]);
                    Int32 player1Time = Int32.Parse(numbers[1]);
                    Int32 player2Time = Int32.Parse(numbers[2]);
                    Int32 putDownsCoordinatesCount = Int32.Parse(numbers[3]);

                    // Check for wrong table size.
                    Boolean isBadTableSize = true;
                    for (Int32 i = 0; i < _acceptableGameTableSizeArray.GetLength(0); ++i)
                    {
                        if (tableSize == _acceptableGameTableSizeArray[0])
                        {
                            isBadTableSize = false;
                        }
                    }
                    if (isBadTableSize)
                    {
                        //TODO: Not like this.
                        throw new ReversiDataException("Source is public async Task<ReversiTable> Load(String path) in public class ReversiFileDataAccess : IReversiDataAccess.", "Not supported table size. 'tableSize' ", ReversiDataExceptionType.FormatException); ;
                    }

                    // Creating the game descriptive data class.
                    ReversiGameDescriptiveData data = new ReversiGameDescriptiveData(tableSize, player1Time, player2Time);

                    // Read a line of the file, then split it bye one space.
                    line = await reader.ReadLineAsync();
                    numbers = line.Split(' ');

                    // Setup values of the putDown array.
                    for (Int32 i = 0; i < putDownsCoordinatesCount; i += 2)
                    {
                        Int32 coordinate = Int32.Parse(numbers[i]);
                        data[i] = coordinate;

                        coordinate = Int32.Parse(numbers[i + 1]);
                        data[i + 1] = coordinate;
                    }

                    return data;
                }
            }
            catch (Exception e)
            {
                if (!(e is ReversiDataException))
                {
                    // To send the type of the exception to 'ReversiDataException'.
                    ReversiDataExceptionType exceptionType = ReversiDataExceptionType.UnknownException;

                    //TODO: ArgumentNullException can throne by Prase() and by StreamReader(String). We have to find out which one throwed it.
                    // The 'Parse()' function exceptions.
                    if (e is ArgumentNullException || e is FormatException || e is OverflowException)
                    {
                        exceptionType = ReversiDataExceptionType.FormatException;
                        throw new ReversiDataException(e.Source, e.Message, exceptionType);
                    }

                    // The 'ReadLineAsync()' function exceptions.
                    if (e is ArgumentOutOfRangeException || e is ObjectDisposedException || e is InvalidOperationException)
                    {
                        exceptionType = ReversiDataExceptionType.StreamReaderException;
                        throw new ReversiDataException(e.Source, e.Message, exceptionType);
                    }

                    // The StreamReader(String) constructior exception.
                    if (e is ArgumentException || e is ArgumentNullException || e is FileNotFoundException || e is DirectoryNotFoundException || e is IOException)
                    {
                        exceptionType = ReversiDataExceptionType.StreamReaderException;
                        throw new ReversiDataException(e.Source, e.Message, exceptionType);
                    }

                    throw new ReversiDataException(e.Source, e.Message, exceptionType);
                }

                throw e;
            }
        }

        /// <summary>
        /// Saving file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="data">All the data to recover the game state. The game table size, the put downs and the players game times.</param>
        public async Task Save(String path, ReversiGameDescriptiveData data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path)) // opening file
                {
                    // Writing the fist line with the size if the table, with the players played times and with the coordinats count.
                    writer.Write(data.TableSize.ToString() + " " + data.Player1Time.ToString() + " " + data.Player2Time.ToString() + " " + data.CoordinatesCount.ToString()); // kiírjuk a méreteket
                    await writer.WriteLineAsync();

                    // Writing the second line with the coordinates.
                    int coordinateCountMinusTwo = data.CoordinatesCount - 2;
                    for (Int32 i = 0; i <= coordinateCountMinusTwo; ++i)
                    {
                        await writer.WriteAsync(data[i].ToString() + " " + data[i + 1].ToString() + " ");
                    }
                    await writer.WriteAsync(data[coordinateCountMinusTwo].ToString() + " " + data[coordinateCountMinusTwo + 1].ToString());
                }
            }
            catch (Exception e)
            {
                // To send the type of the exception to 'ReversiDataException'.
                ReversiDataExceptionType exceptionType = ReversiDataExceptionType.UnknownException;
                //TODO: ArgumentNullException can throne by Prase() and by StreamReader(String). We have to find out which one throwed it.
                // The 'Parse()' function exceptions.
                if (e is ArgumentNullException || e is FormatException || e is OverflowException)
                {
                    exceptionType = ReversiDataExceptionType.FormatException;
                    throw new ReversiDataException(e.Source, e.Message, exceptionType);
                }

                // The 'ReadLineAsync()' function exceptions.
                if (e is ArgumentOutOfRangeException || e is ObjectDisposedException || e is InvalidOperationException)
                {
                    exceptionType = ReversiDataExceptionType.StreamReaderException;
                    throw new ReversiDataException(e.Source, e.Message, exceptionType);
                }

                // The StreamReader(String) constructior exception.
                if (e is ArgumentException || e is ArgumentNullException || e is FileNotFoundException || e is DirectoryNotFoundException
                    || e is IOException || e is UnauthorizedAccessException || e is SecurityException)
                {
                    exceptionType = ReversiDataExceptionType.StreamReaderException;
                    throw new ReversiDataException(e.Source, e.Message, exceptionType);
                }

                throw new ReversiDataException(e.Source, e.Message, exceptionType);
            }
        }

        #endregion

    }
}
