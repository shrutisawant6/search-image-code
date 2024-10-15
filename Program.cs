using Sage.Constants;
using Sage.Models;
using System.Drawing;

class Program
{

    static void Main()
    {
        SearchImage();
    }


    static void SearchImage()
    {
        try
        {
            #region Initial details and colour interpretations

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("┌─────────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ Directives:                                                                             │");
            Console.WriteLine("│ @ Image to be uploaded should have least white background for better results.           │");
            Console.WriteLine("│ @ [ Resultant Match ] Will display 10 images with most similarity, as per the search.   │");
            Console.WriteLine("│ @ Colour interpretations:                                                               │");
            WriteColoredLine("│  * Colour Green - for exact match                                                       │", ConsoleColor.Green);
            WriteColoredLine("│  * Colour Blue - for similarity at least 90%                                            │", ConsoleColor.Blue);
            WriteColoredLine("│  * Colour Magenta - for similarity at least 80%                                         │", ConsoleColor.Magenta);
            WriteColoredLine("│  * Colour Yellow - for similarity at least 45%                                          │", ConsoleColor.Yellow);
            WriteColoredLine("│  * Colour Gray - for very minimal or no similarity                                      │", ConsoleColor.DarkGray);
            WriteColoredLine("│  * Colour Red - where image processing has failed                                       │", ConsoleColor.Red);
            Console.WriteLine("└─────────────────────────────────────────────────────────────────────────────────────────┘");

            #endregion


            #region Enter and validate folder path
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"Enter the folder path to be searched. Eg: C:\\Temp\\Folder1 >>");
            string folderPath = Console.ReadLine();
            if (string.IsNullOrEmpty(folderPath))
            {
                string folderLocationEmpty = "Validate the folder path.";
                Console.WriteLine(folderLocationEmpty);
                return;
            }
            #endregion


            #region Enter and validate image path
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"Enter the image path, including name, that needs searching. Eg: C:\\Temp\\Folder1\\Image.png >>");
            string imageTobeMatched = Console.ReadLine();
            if (string.IsNullOrEmpty(folderPath))
            {
                string folderLocationEmpty = "Validate the image path.";
                Console.WriteLine(folderLocationEmpty);
                return;
            }
            Bitmap bitmapOfImageTobeMatched = new(imageTobeMatched);
            #endregion


            #region Check number of files within a folder
            Console.WriteLine("");
            Console.WriteLine("");
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories);
            var fileCount = files?.Count();
            if (fileCount == null || fileCount <= 0)
            {
                Console.WriteLine($"The {folderPath} is empty.");
                return;
            }
            else
                Console.WriteLine($"Count of files in the folder {folderPath} = {fileCount}");
            #endregion


            #region Check for number of images within the files of folder
            Console.WriteLine("");
            Console.WriteLine("");
            var imageFilesWithinFolderCount = files?.Where(f => IsImage(f)).Count();
            if (imageFilesWithinFolderCount == null || imageFilesWithinFolderCount <= 0)
            {
                Console.WriteLine($"No image files within {folderPath}");
                return;
            }
            else
                Console.WriteLine($"Count of image files in the folder {folderPath} to be matched with = {imageFilesWithinFolderCount}");
            #endregion


            #region Start the search
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"Search has began...");

            Console.WriteLine("──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────");
            Console.WriteLine(" Percentage | File    ");
            Console.WriteLine("──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────");

            var fileContents = new List<FileContent>();

            foreach (var file in files)
            {
                try
                {
                    var content= FindSimilarities(file, bitmapOfImageTobeMatched);
                    if (content != null)
                        fileContents.Add(content);
                }
                catch (Exception ex)
                {
                    WriteColoredLine($" N/A        | {file}", ConsoleColor.Red);
                }
            }
            #endregion


            #region Get resultant images matched
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            WriteColoredLine("[ Resultant Match ]:", ConsoleColor.White);
            Console.WriteLine("──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────");
            Console.WriteLine(" Percentage | File    ");
            Console.WriteLine("──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────");


            var resultantMatches = fileContents.OrderByDescending(f => f.SimilarityScore).ToList();

            if (fileContents.Any(f => f.SimilarityScore >= Score.Value9999))
                WriteColoredLine("Voila! You have an exact match.", ConsoleColor.Green);
            else if (fileContents.Count(f => f.SimilarityScore >= Score.Value9m) > 10)
                WriteColoredLine("There are couple of images that have similarity at least 90%. Either a similar image is stored at multiple places or the image structure is commonly used.", ConsoleColor.Blue);
            else if (fileContents.Count(f => f.SimilarityScore >= Score.Value8m) > 10)
                WriteColoredLine("There are couple of images that have similarity at least 80%. Either a similar image is stored at multiple places or the image struture is commonly used.", ConsoleColor.Magenta);

            resultantMatches = fileContents.OrderByDescending(f => f.SimilarityScore)
                                                       .Take(10)
                                                       .ToList();

            foreach (var file in resultantMatches)
                GetResultantMatch(file);

            WriteColoredLine("For more details, check the image paths and their matching score, in the list above.", ConsoleColor.White);

            #endregion
        }
        catch (Exception)
        {
            WriteColoredLine($"Something is not right! Check your folder path/image and try again.", ConsoleColor.Red);
        }

        Console.ReadLine();
    }


    static FileContent? FindSimilarities(string file, Bitmap bitmapOfImageTobeMatched)
    {
        if (IsImage(file))
        {
            using (Bitmap img1 = new(file))
            {
                decimal similarity = CompareHistograms(img1, bitmapOfImageTobeMatched);

                if (similarity >= Score.Value9m)
                {
                    var formatSimilarityScore = $"{(similarity * 100):00.00}";
                    if (similarity >= Score.Value9999)
                    {
                        formatSimilarityScore = $"{(similarity * 100):000.00}";
                        WriteColoredLine($" {formatSimilarityScore}%    | {file}", ConsoleColor.Green);
                    }
                    else
                        WriteColoredLine($" {formatSimilarityScore}%     | {file}", ConsoleColor.Blue);
                }
                else if (similarity >= Score.Value8m)
                    FormatEachline(similarity, file, ConsoleColor.Magenta);
                else if (similarity >= Score.Value45)
                    FormatEachline(similarity, file, ConsoleColor.Yellow);
                else
                    FormatEachline(similarity, file, ConsoleColor.DarkGray);

                return new FileContent { SimilarityScore = similarity, Path = file };
            }
        }
        return null;
    }


    static void GetResultantMatch(FileContent file)
    {
        if (file.SimilarityScore >= Score.Value9m)
        {
            var formatSimilarityScore = $"{(file.SimilarityScore * 100):00.00}";
            if (file.SimilarityScore >= Score.Value9999)
            {
                formatSimilarityScore = $"{(file.SimilarityScore * 100):000.00}";
                WriteColoredLine($" {formatSimilarityScore}%    | {file.Path}", ConsoleColor.Green);
            }
            else
                WriteColoredLine($" {formatSimilarityScore}%     | {file.Path}", ConsoleColor.Blue);

        }
        else if (file.SimilarityScore >= Score.Value8m)
            FormatEachline(file.SimilarityScore, file.Path, ConsoleColor.Magenta);
        else if (file.SimilarityScore >= Score.Value45)
            FormatEachline(file.SimilarityScore, file.Path, ConsoleColor.Yellow);
        else
            FormatEachline(file.SimilarityScore, file.Path, ConsoleColor.DarkGray);
    }


    static bool IsImage(string filePath)
    {
        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".jfif", ".webp", ".heif", ".heic", ".avif" };
        return Array.Exists(imageExtensions, ext => ext.Equals(Path.GetExtension(filePath), StringComparison.OrdinalIgnoreCase));
    }


    static decimal CompareHistograms(Bitmap img1, Bitmap img2)
    {
        // Resize images to the same size for comparison
        Bitmap resizedImage1 = new(img1, new Size(256, 256));
        Bitmap resizedImage2 = new(img2, new Size(256, 256));

        // Compute histograms for both images
        int[] imageHistogram1 = ComputeHistogram(resizedImage1);//histogram = gray-scale value distribution showing the frequency of occurrence of each gray-level value
        int[] imageHistogram2 = ComputeHistogram(resizedImage2);

        // Compare histograms using a similarity metric
        decimal similarity = 0;
        decimal dotProduct = 0, imageMagnitude1 = 0, imageMagnitude2 = 0;
        for (int i = 0; i < imageHistogram1.Length; i++)
        {
            dotProduct += (decimal)imageHistogram1[i] * (decimal)imageHistogram2[i];// A histogram dot product of an image = method of estimating the similarity between images using binary vectors that represent the image
            imageMagnitude1 += (decimal)Math.Pow(imageHistogram1[i], 2);// gradient magnitude |G| = [Gx^2 + Gy^2]
            imageMagnitude2 += (decimal)Math.Pow(imageHistogram2[i], 2);
        }

        //Magnitude of an image(gradient magnitude) = a measure of how quickly an image's intensity changes.
        imageMagnitude1 = (decimal)Math.Sqrt((double)imageMagnitude1);
        imageMagnitude2 = (decimal)Math.Sqrt((double)imageMagnitude2);

        if (imageMagnitude1 > 0 && imageMagnitude2 > 0)
        {
            similarity = dotProduct / (imageMagnitude1 * imageMagnitude2);
        }

        return similarity;
    }


    static int[] ComputeHistogram(Bitmap image)
    {
        int[] histogram = new int[256];

        for (int w = 0; w < image.Width; w++)
        {
            for (int h = 0; h < image.Height; h++)
            {
                System.Drawing.Color pixel = image.GetPixel(w, h);

                //https://www.tutorialspoint.com/dip/grayscale_to_rgb_conversion.htm#:~:text=New%20grayscale%20image%20%3D%20(%20(0.3,and%20Blue%20has%20contributed%2011%25.
                //Grayscale image = ((0.3 * R) + (0.59 * G) + (0.11 * B))
                //Red has contribute 30%
                //Green has contributed 59%
                //Blue has contributed 11%.
                int brightness = (int)(0.3 * pixel.R + 0.59 * pixel.G + 0.11 * pixel.B); // Grayscale value

                histogram[brightness]++;
            }
        }

        return histogram;
    }


    static void FormatEachline(decimal value, string path, ConsoleColor color)
    {
        WriteColoredLine($" {(value * 100):00.00}%     | {path}", color);
    }


    private static void WriteColoredLine(string message, ConsoleColor color = ConsoleColor.Green)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.WriteLine("");
        Console.ResetColor();
    }
}