namespace Zadanie_64;

class Program
{
    static void Main(string[] args)
    {
        int pictureSize = 20;
        string[] pictureData = File.ReadAllText("dane_obrazki.txt")
            .Split(new string[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        int reverseImageCount = 0;
        int highestBlackPixelCount = 0;
        bool foundFirstRecurringImage = false;
        string[] firstRecurringImage = [];
        int correctImageCount = 0;
        int unrepairableImageCount = 0;
        int highestParityBitCount = 0;
        int imageNumber = 0;
        int recurringImageCount = 0;
        List<int> repairableImageIndices = [];
        List<(int?, int?)> repairableImageRowAndColumnNumber = [];

        bool enableVisualization = true;

        foreach (var picture in pictureData)
        {
            imageNumber++;
            // 64.1 ->
            string[] rowsWithParityBits = GetRowsFromPicture(picture);
            string[] rows = rowsWithParityBits
                .Select(row => row[..pictureSize])
                .Take(rowsWithParityBits.Length - 1)
                .ToArray();

            var (isReverse, blackPixelCount) = GetReverseStatusAndBlackPixelCount(rows);
            if (isReverse)
                reverseImageCount++;
            if (blackPixelCount > highestBlackPixelCount)
                highestBlackPixelCount = blackPixelCount;
            // <- 64.1

            // 64.2 ->
            var topLeftCorner = GetImageChunk(0, pictureSize / 2, 0, pictureSize / 2, rows);
            var topRightCorner = GetImageChunk(0, pictureSize / 2, pictureSize / 2, pictureSize, rows);
            var bottomLeftCorner = GetImageChunk(pictureSize / 2, pictureSize, 0, pictureSize / 2, rows);
            var bottomRightCorner = GetImageChunk(pictureSize / 2, pictureSize, pictureSize / 2, pictureSize, rows);
            if (CheckChunkEquality(topLeftCorner, topRightCorner) &&
                CheckChunkEquality(topLeftCorner, bottomLeftCorner) &&
                CheckChunkEquality(topLeftCorner, bottomRightCorner))
            {
                if (!foundFirstRecurringImage)
                {
                    foundFirstRecurringImage = true;
                    firstRecurringImage = rows;
                }

                recurringImageCount++;
                if (enableVisualization)
                    DrawImage(rows, $"\n{imageNumber} - obrazek rekurencyjny");
            }
            // <- 64.2

            // 64.3 ->
            List<int> rowErrorIndices = new();
            List<int> colErrorIndices = new();
            CheckRowsParity(rowsWithParityBits, ref rowErrorIndices);
            CheckColumnParity(rowsWithParityBits, pictureSize, ref colErrorIndices);
            bool isCorrect = rowErrorIndices.Count == 0 && colErrorIndices.Count == 0;
            bool isRepairable = !isCorrect && rowErrorIndices.Count <= 1 && colErrorIndices.Count <= 1;
            bool isUnrepairable = !isCorrect && !isRepairable;
            if (isCorrect)
                correctImageCount++;
            else if (isRepairable)
            {
                repairableImageIndices.Add(imageNumber);

                int? rowIndex = rowErrorIndices.Any() ? rowErrorIndices.FirstOrDefault() : null;
                int? colIndex = colErrorIndices.Any() ? colErrorIndices.FirstOrDefault() : null;
                repairableImageRowAndColumnNumber.Add((rowIndex, colIndex));

                if (enableVisualization)
                    DrawImageWithErrors(rows, rowErrorIndices, colErrorIndices, $"\n{imageNumber} - obrazek naprawialny");
            }
            else if (isUnrepairable)
                unrepairableImageCount++;

            int totalErrors = rowErrorIndices.Count + colErrorIndices.Count;
            if (totalErrors > highestParityBitCount)
                highestParityBitCount = totalErrors;
            // <- 64.3
        }


        using var writer = new StreamWriter("wyniki_obrazki.txt");
        writer.WriteLine("64.1");
        writer.WriteLine($"Liczba rewersów: {reverseImageCount}");
        writer.WriteLine($"Największa liczba czarnych pikseli: {highestBlackPixelCount}");
        writer.WriteLine("\n64.2");
        writer.WriteLine($"Liczba obrazków rekurencyjnych: {recurringImageCount}");
        if (foundFirstRecurringImage)
        {
            writer.WriteLine("Pierwszy powtarzający się obrazek:\n");
            foreach (var row in firstRecurringImage)
            {
                writer.WriteLine(row);
            }
        }

        writer.WriteLine("\n64.3");
        writer.WriteLine($"Liczba poprawnych obrazków: {correctImageCount}");
        writer.WriteLine($"Liczba obrazków naprawialnych: {repairableImageIndices.Count}");
        writer.WriteLine($"Liczba obrazków nienaprawialnych: {unrepairableImageCount}");
        writer.WriteLine($"Największa liczba błędów bitów parzystości w jednym obrazku: {highestParityBitCount}");
        writer.WriteLine("Naprawialne obrazki:");
        for (int i = 0; i < repairableImageIndices.Count; i++)
        {
            string rowAndCol = string.Empty;
            if (repairableImageRowAndColumnNumber[i].Item1 != null)
                rowAndCol += repairableImageRowAndColumnNumber[i].Item1;
            else
                rowAndCol += "-";

            rowAndCol += ", ";

            if (repairableImageRowAndColumnNumber[i].Item2 != null)
                rowAndCol += repairableImageRowAndColumnNumber[i].Item2;
            else
                rowAndCol += "-";

            writer.WriteLine($"{repairableImageIndices[i]}: {rowAndCol}");
        }
    }


    static (bool, int) GetReverseStatusAndBlackPixelCount(string[] rows)
    {
        int blackPixelsCount = 0;
        int whitePixelsCount = 0;
        foreach (var row in rows)
        {
            foreach (var bit in row)
            {
                if (bit == '1')
                    blackPixelsCount++;
                else
                    whitePixelsCount++;
            }
        }

        return (blackPixelsCount > whitePixelsCount, blackPixelsCount);
    }

    static string[] GetRowsFromPicture(string picture)
    {
        return picture.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
    }

    static string[] GetImageChunk(int rowStart, int rowEnd, int colStart, int colEnd, string[] rows)
    {
        List<string> chunk = [];
        for (int i = rowStart; i < rowEnd; i++)
        {
            chunk.Add(rows[i][colStart..colEnd]);
        }

        return chunk.ToArray();
    }

    static bool CheckChunkEquality(string[] firstChunk, string[] secondChunk)
    {
        for (int i = 0; i < firstChunk.Length; i++)
        {
            if (firstChunk[i] != secondChunk[i])
                return false;
        }

        return true;
    }


    static void CheckRowsParity(string[] rows, ref List<int> rowErrorIndices)
    {
        foreach (var row in rows.Take(rows.Length - 1))
        {
            bool isRowParityCorrect = CheckRowParity(row);
            if (!isRowParityCorrect)
                rowErrorIndices.Add(Array.IndexOf(rows, row));
        }
    }

    static bool CheckRowParity(string row)
    {
        int blackPixelCount = row.Take(row.Length - 1).Count(bit => bit == '1');
        char parityBit = row.Last();
        bool isEven = blackPixelCount % 2 == 0;
        return CheckParityCorrectness(isEven, parityBit);
    }

    static void CheckColumnParity(string[] rows, int pictureSize, ref List<int> colErrorIndices)
    {
        for (int col = 0; col < pictureSize; col++)
        {
            int blackPixelCount = 0;
            for (int row = 0; row < pictureSize; row++)
            {
                if (rows[row][col] == '1')
                    blackPixelCount++;
            }

            char parityBit = rows[pictureSize][col];
            bool isEven = blackPixelCount % 2 == 0;

            if (!CheckParityCorrectness(isEven, parityBit))
                colErrorIndices.Add(col);
        }
    }

    static bool CheckParityCorrectness(bool isEven, char parityBit)
    {
        return isEven && parityBit == '0' || !isEven && parityBit == '1';
    }


    static void DrawImage(string[] rows, string title = "")
    {
        if (!string.IsNullOrEmpty(title))
            Console.WriteLine(title);

        foreach (var row in rows)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            foreach (var bit in row)
            {
                Console.ForegroundColor = bit == '1' ? ConsoleColor.Black : ConsoleColor.DarkGray;
                Console.Write("███");
            }

            Console.Write("\n");
        }

        Console.ResetColor();
    }

    static void DrawImageWithErrors(string[] rows, List<int> rowErrorIndices, List<int> colErrorIndices, string title = "")
    {
        if (!string.IsNullOrEmpty(title))
            Console.WriteLine(title);

        int rowCount = 0;
        int rowLength = rows.FirstOrDefault()!.Length;
        bool hasCrossingPoint = rowErrorIndices.Count == 1 && colErrorIndices.Count == 1;
        List<int> errorsOnCols = [];
        bool isCrossingPoint = false;
        int rowIndex = 0;
        foreach (var row in rows)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            int colCount = 0;
            foreach (var bit in row)
            {
                if (hasCrossingPoint && rowErrorIndices.Contains(rowCount))
                {
                    if (colErrorIndices.Contains(colCount))
                    {
                        isCrossingPoint = true;
                    }
                }

                if (isCrossingPoint)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    isCrossingPoint = false;
                }
                else
                {
                    Console.ForegroundColor = bit == '1' ? ConsoleColor.Black : ConsoleColor.DarkGray;
                }

                Console.Write("███");
                if (colErrorIndices.Contains(colCount))
                {
                    errorsOnCols.Add(colCount);
                }

                colCount++;
            }

            Console.ForegroundColor = rowErrorIndices.Contains(rowCount) ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
            Console.Write("███");
            rowCount++;
            Console.Write("\n");
        }

        if (errorsOnCols.Any())
        {
            for (var i = 0; i < rowLength; i++)
            {
                Console.ForegroundColor = errorsOnCols.Contains(i) ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
                Console.Write("███");
            }
        }

        Console.Write("\n");
        Console.ResetColor();
    }
}