using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MatrixCalculator {
  public class MatrixException : Exception {
    public MatrixException() : base() { }
    public MatrixException(string message) : base(message) { }
    public MatrixException(string message, Exception inner) : base(message, inner) { }
  }

  public class InvalidMatrixSizeException : MatrixException {
    public InvalidMatrixSizeException() : base(" Invalid matrix size ") { }
    public InvalidMatrixSizeException(string message) : base(message) { }
  }

  public class MatrixDimensionMismatchException : MatrixException {
    public MatrixDimensionMismatchException() : base(" Matrix dimensions do not match ") { }
    public MatrixDimensionMismatchException(string message) : base(message) { }
  }

  public class SingularMatrixException : MatrixException {
    public SingularMatrixException() : base(" Matrix is singular (determinant = 0) ") { }
    public SingularMatrixException(string message) : base(message) { }
  }

  public class SquareMatrix : IComparable, ICloneable, IEnumerable<double> {
    private double[,] _matrixData;
    private int _matrixSize;
    private double _cachedDeterminant;

    public int Size {
      get { return MatrixSize; }
    }

    public double this[int rowIndex, int columnIndex] {
      get {
        if (rowIndex < 0 || rowIndex >= MatrixSize || columnIndex < 0 || columnIndex >= MatrixSize)
          throw new IndexOutOfRangeException(" Indices are outside the matrix boundaries ");
        return MatrixData[rowIndex, columnIndex];
      }
      set {
        if (rowIndex < 0 || rowIndex >= MatrixSize || columnIndex < 0 || columnIndex >= MatrixSize)
          throw new IndexOutOfRangeException(" Indices are outside the matrix boundaries ");
        MatrixData[rowIndex, columnIndex] = value;
        _cachedDeterminant = double.NaN;
      }
    }

    public double Determinant {
      get {
        if (double.IsNaN(_cachedDeterminant))
          _cachedDeterminant = CalculateDeterminant();
        return _cachedDeterminant;
      }
    }

    public double[,] MatrixData { get => _matrixData; private set => _matrixData = value; }
    public int MatrixSize { get => _matrixSize; private set => _matrixSize = value; }

    public SquareMatrix() : this(2) { }

    public SquareMatrix(int size) {
      if (size <= 0)
        throw new InvalidMatrixSizeException(" Matrix size must be a positive number ");

      MatrixSize = size;
      MatrixData = new double[MatrixSize, MatrixSize];
      _cachedDeterminant = double.NaN;
    }

    public SquareMatrix(int size, bool randomFill) : this(size) {
      if (randomFill) {
        Random randomGenerator = new Random();
        for (int currentRowIndex = 0; currentRowIndex < MatrixSize; ++currentRowIndex)
          for (int currentColumnIndex = 0; currentColumnIndex < MatrixSize; ++currentColumnIndex)
            MatrixData[currentRowIndex, currentColumnIndex] = randomGenerator.Next(-10, 11);
      }
    }

    public SquareMatrix(SquareMatrix otherMatrix) {
      if (otherMatrix == null)
        throw new ArgumentNullException(nameof(otherMatrix));

      MatrixSize = otherMatrix.MatrixSize;
      MatrixData = new double[MatrixSize, MatrixSize];

      for (int currentRowIndex = 0; currentRowIndex < MatrixSize; ++currentRowIndex)
        for (int currentColumnIndex = 0; currentColumnIndex < MatrixSize; ++currentColumnIndex)
          MatrixData[currentRowIndex, currentColumnIndex] = otherMatrix.MatrixData[currentRowIndex, currentColumnIndex];

      _cachedDeterminant = otherMatrix._cachedDeterminant;
    }

    public SquareMatrix(double[,] twoDimensionalArray) {
      if (twoDimensionalArray == null)
        throw new ArgumentNullException(nameof(twoDimensionalArray));

      int rowsCount = twoDimensionalArray.GetLength(0);
      int columnsCount = twoDimensionalArray.GetLength(1);

      if (rowsCount != columnsCount)
        throw new InvalidMatrixSizeException(" Matrix must be square ");

      MatrixSize = rowsCount;
      MatrixData = new double[MatrixSize, MatrixSize];

      for (int currentRowIndex = 0; currentRowIndex < MatrixSize; ++currentRowIndex)
        for (int currentColumnIndex = 0; currentColumnIndex < MatrixSize; ++currentColumnIndex)
          MatrixData[currentRowIndex, currentColumnIndex] = twoDimensionalArray[currentRowIndex, currentColumnIndex];

      _cachedDeterminant = double.NaN;
    }

    public static SquareMatrix operator +(SquareMatrix matrix) {
      if (matrix == null)
        throw new ArgumentNullException(nameof(matrix));
      return new SquareMatrix(matrix);
    }

    public static SquareMatrix operator +(SquareMatrix leftMatrix, SquareMatrix rightMatrix) {
      if (leftMatrix == null || rightMatrix == null)
        throw new ArgumentNullException(" Matrix cannot be null ");

      if (leftMatrix.MatrixSize != rightMatrix.MatrixSize)
        throw new MatrixDimensionMismatchException(" Matrix dimensions must match for addition ");

      SquareMatrix resultMatrix = new SquareMatrix(leftMatrix.MatrixSize);
      for (int currentRowIndex = 0; currentRowIndex < leftMatrix.MatrixSize; ++currentRowIndex)
        for (int currentColumnIndex = 0; currentColumnIndex < leftMatrix.MatrixSize; ++currentColumnIndex)
          resultMatrix[currentRowIndex, currentColumnIndex] = leftMatrix[currentRowIndex, currentColumnIndex] + rightMatrix[currentRowIndex, currentColumnIndex];
      return resultMatrix;
    }

    public static SquareMatrix operator *(SquareMatrix leftMatrix, SquareMatrix rightMatrix) {
      if (leftMatrix == null || rightMatrix == null)
        throw new ArgumentNullException(" Matrix cannot be null ");

      if (leftMatrix.MatrixSize != rightMatrix.MatrixSize)
        throw new MatrixDimensionMismatchException(" Matrix dimensions must match for multiplication ");

      SquareMatrix resultMatrix = new SquareMatrix(leftMatrix.MatrixSize);
      for (int currentRowIndex = 0; currentRowIndex < leftMatrix.MatrixSize; ++currentRowIndex) {
        for (int currentColumnIndex = 0; currentColumnIndex < leftMatrix.MatrixSize; ++currentColumnIndex) {
          double sum = 0;
          for (int multiplicationIndex = 0; multiplicationIndex < leftMatrix.MatrixSize; ++multiplicationIndex)
            sum += leftMatrix[currentRowIndex, multiplicationIndex] * rightMatrix[multiplicationIndex, currentColumnIndex];
          resultMatrix[currentRowIndex, currentColumnIndex] = sum;
        }
      }
      return resultMatrix;
    }

    public static SquareMatrix operator *(SquareMatrix matrix, double scalarValue) {
      if (matrix == null)
        throw new ArgumentNullException(nameof(matrix));

      SquareMatrix resultMatrix = new SquareMatrix(matrix.MatrixSize);
      for (int currentRowIndex = 0; currentRowIndex < matrix.MatrixSize; ++currentRowIndex)
        for (int currentColumnIndex = 0; currentColumnIndex < matrix.MatrixSize; ++currentColumnIndex)
          resultMatrix[currentRowIndex, currentColumnIndex] = matrix[currentRowIndex, currentColumnIndex] * scalarValue;
      return resultMatrix;
    }

    public static SquareMatrix operator *(double scalarValue, SquareMatrix matrix) {
      return matrix * scalarValue;
    }

    public static bool operator ==(SquareMatrix leftMatrix, SquareMatrix rightMatrix) {
      if (!(ReferenceEquals(leftMatrix, null) && ReferenceEquals(rightMatrix, null))) {
        if (ReferenceEquals(leftMatrix, null) || ReferenceEquals(rightMatrix, null))
          return false;

        if (leftMatrix.MatrixSize != rightMatrix.MatrixSize)
          return false;

        for (int currentRowIndex = 0; currentRowIndex < leftMatrix.MatrixSize; ++currentRowIndex)
          for (int currentColumnIndex = 0; currentColumnIndex < leftMatrix.MatrixSize; ++currentColumnIndex)
            if (Math.Abs(leftMatrix[currentRowIndex, currentColumnIndex] - rightMatrix[currentRowIndex, currentColumnIndex]) > 1e-10)
              return false;
        return true;
      }

      return true;
    }

    public static bool operator !=(SquareMatrix leftMatrix, SquareMatrix rightMatrix) {
      return !(leftMatrix == rightMatrix);
    }

    public static bool operator >(SquareMatrix leftMatrix, SquareMatrix rightMatrix) {
      if (leftMatrix == null || rightMatrix == null)
        throw new ArgumentNullException(" Matrix cannot be null ");

      if (leftMatrix.MatrixSize != rightMatrix.MatrixSize)
        throw new MatrixDimensionMismatchException(" Matrix dimensions must match for comparison ");

      return leftMatrix.SumOfElements() > rightMatrix.SumOfElements();
    }

    public static bool operator <(SquareMatrix leftMatrix, SquareMatrix rightMatrix) {
      if (leftMatrix == null || rightMatrix == null)
        throw new ArgumentNullException(" Matrix cannot be null ");

      if (leftMatrix.MatrixSize != rightMatrix.MatrixSize)
        throw new MatrixDimensionMismatchException(" Matrix dimensions must match for comparison ");

      return leftMatrix.SumOfElements() < rightMatrix.SumOfElements();
    }

    public static bool operator >=(SquareMatrix leftMatrix, SquareMatrix rightMatrix) {
      return !(leftMatrix < rightMatrix);
    }

    public static bool operator <=(SquareMatrix leftMatrix, SquareMatrix rightMatrix) {
      return !(leftMatrix > rightMatrix);
    }

    public static bool operator true(SquareMatrix matrix) {
      if (matrix is null)
        return false;

      for (int currentRowIndex = 0; currentRowIndex < matrix.MatrixSize; ++currentRowIndex)
        for (int currentColumnIndex = 0; currentColumnIndex < matrix.MatrixSize; ++currentColumnIndex)
          if (Math.Abs(matrix[currentRowIndex, currentColumnIndex]) > 1e-10)
            return true;
      return false;
    }

    public static bool operator false(SquareMatrix matrix) {
      if (matrix is null)
        return true;

      for (int currentRowIndex = 0; currentRowIndex < matrix.MatrixSize; ++currentRowIndex)
        for (int currentColumnIndex = 0; currentColumnIndex < matrix.MatrixSize; ++currentColumnIndex)
          if (Math.Abs(matrix[currentRowIndex, currentColumnIndex]) > 1e-10)
            return false;
      return true;
    }

    public static implicit operator SquareMatrix(double value) {
      SquareMatrix resultMatrix = new SquareMatrix(1);
      resultMatrix[0, 0] = value;
      return resultMatrix;
    }

    public static explicit operator double(SquareMatrix matrix) {
      if (matrix == null)
        throw new ArgumentNullException(nameof(matrix));
      return matrix.Determinant;
    }

    public static implicit operator string(SquareMatrix matrix) {
      if (matrix == null)
        return "null";
      return matrix.ToString();
    }

    private double CalculateDeterminant() {
      return CalculateDeterminantRecursive(MatrixData, MatrixSize);
    }

    private double CalculateDeterminantRecursive(double[,] matrix, int currentSize) {
      if (currentSize == 1)
        return matrix[0, 0];

      if (currentSize == 2)
        return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

      double determinant = 0;
      for (int currentColumnIndex = 0; currentColumnIndex < currentSize; ++currentColumnIndex) {
        double[,] subMatrix = new double[currentSize - 1, currentSize - 1];
        for (int subMatrixRowIndex = 1; subMatrixRowIndex < currentSize; ++subMatrixRowIndex) {
          for (int originalColumnIndex = 0, subMatrixColumnIndex = 0; originalColumnIndex < currentSize; ++originalColumnIndex) {
            if (originalColumnIndex == currentColumnIndex) continue;
            subMatrix[subMatrixRowIndex - 1, subMatrixColumnIndex] = matrix[subMatrixRowIndex, originalColumnIndex];
            ++subMatrixColumnIndex;
          }
        }
        determinant += matrix[0, currentColumnIndex] * Math.Pow(-1, currentColumnIndex) *
                      CalculateDeterminantRecursive(subMatrix, currentSize - 1);
      }
      return determinant;
    }

    public SquareMatrix Inverse() {
      double determinantValue = this.Determinant;
      if (Math.Abs(determinantValue) < 1e-10)
        throw new SingularMatrixException(" Cannot find inverse matrix (determinant is 0) ");

      SquareMatrix resultMatrix = new SquareMatrix(MatrixSize);

      if (MatrixSize == 1) {
        resultMatrix[0, 0] = 1.0 / MatrixData[0, 0];
        return resultMatrix;
      }

      for (int currentRowIndex = 0; currentRowIndex < MatrixSize; ++currentRowIndex) {
        for (int currentColumnIndex = 0; currentColumnIndex < MatrixSize; ++currentColumnIndex) {
          resultMatrix[currentRowIndex, currentColumnIndex] = Math.Pow(-1, currentRowIndex + currentColumnIndex) *
                                     CalculateMinor(currentColumnIndex, currentRowIndex) / determinantValue;
        }
      }

      return resultMatrix;
    }

    private double CalculateMinor(int rowToRemove, int columnToRemove) {
      double[,] minorMatrix = new double[MatrixSize - 1, MatrixSize - 1];
      for (int originalRowIndex = 0, minorRowIndex = 0; originalRowIndex < MatrixSize; ++originalRowIndex) {
        if (originalRowIndex == rowToRemove) continue;
        for (int originalColumnIndex = 0, minorColumnIndex = 0; originalColumnIndex < MatrixSize; ++originalColumnIndex) {
          if (originalColumnIndex == columnToRemove) continue;
          minorMatrix[minorRowIndex, minorColumnIndex] = MatrixData[originalRowIndex, originalColumnIndex];
          ++minorColumnIndex;
        }
        ++minorRowIndex;
      }
      return CalculateDeterminantRecursive(minorMatrix, MatrixSize - 1);
    }

    private double SumOfElements() {
      double sum = 0;
      for (int currentRowIndex = 0; currentRowIndex < MatrixSize; ++currentRowIndex)
        for (int currentColumnIndex = 0; currentColumnIndex < MatrixSize; ++currentColumnIndex)
          sum += MatrixData[currentRowIndex, currentColumnIndex];
      return sum;
    }

    public object Clone() {
      return new SquareMatrix(this);
    }

    public SquareMatrix DeepCopy() {
      return new SquareMatrix(this);
    }

    public int CompareTo(object otherObject) {
      if (otherObject == null)
        return 1;

      if (otherObject is SquareMatrix otherMatrix) {
        double thisSum = this.SumOfElements();
        double otherSum = otherMatrix.SumOfElements();

        if (thisSum < otherSum) return -1;
        if (Math.Abs(thisSum - otherSum) < 1e-10) return 0;
        return 1;
      }

      throw new ArgumentException(" Object must be of type SquareMatrix ");
    }

    public IEnumerator<double> GetEnumerator() {
      for (int currentRowIndex = 0; currentRowIndex < MatrixSize; ++currentRowIndex)
        for (int currentColumnIndex = 0; currentColumnIndex < MatrixSize; ++currentColumnIndex)
          yield return MatrixData[currentRowIndex, currentColumnIndex];
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public override bool Equals(object otherObject) {
      if (otherObject is SquareMatrix otherMatrix) {
        return this == otherMatrix;
      }
      return false;
    }

    public override int GetHashCode() {
      return (int)this.SumOfElements();
    }

    public override string ToString() {
      StringBuilder stringBuilder = new StringBuilder();
      for (int currentRowIndex = 0; currentRowIndex < MatrixSize; ++currentRowIndex) {
        for (int currentColumnIndex = 0; currentColumnIndex < MatrixSize; ++currentColumnIndex) {
          stringBuilder.Append($"{MatrixData[currentRowIndex, currentColumnIndex],8:F2} ");
        }
        stringBuilder.AppendLine();
      }
      return stringBuilder.ToString();
    }

    public static SquareMatrix ReadFromConsole(int size) {
      SquareMatrix matrix = new SquareMatrix(size);
      Console.WriteLine($" Enter elements for {size}x{size} matrix: ");
      Console.WriteLine(" (Enter numbers separated by spaces or commas) ");

      for (int currentRowIndex = 0; currentRowIndex < size; ++currentRowIndex) {
        bool validInput = false;
        while (!validInput) {
          Console.Write($" Row {currentRowIndex + 1}: ");
          string inputLine = Console.ReadLine().Trim();

          string[] inputValues = inputLine.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);

          if (inputValues.Length != size) {
            Console.WriteLine($" Error: Expected {size} numbers, but got {inputValues.Length}. Please try again. ");
            continue;
          }

          validInput = true;
          for (int currentColumnIndex = 0; currentColumnIndex < size; ++currentColumnIndex) {
            if (!double.TryParse(inputValues[currentColumnIndex], out double parsedValue)) {
              Console.WriteLine($" Error: '{inputValues[currentColumnIndex]}' is not a valid number. Please try again. ");
              validInput = false;
              break;
            }
            matrix[currentRowIndex, currentColumnIndex] = parsedValue;
          }
        }
      }

      return matrix;
    }
  }

  class Program {
    static void Main(string[] args) {
      Console.OutputEncoding = Encoding.UTF8;
      Console.WriteLine(" === MATRIX CALCULATOR ===\n ");

      while (true) {
        Console.WriteLine(" Select input method: ");
        Console.WriteLine(" 1. Random matrices ");
        Console.WriteLine(" 2. Manual input from keyboard ");
        Console.WriteLine(" 3. Exit ");
        Console.Write(" Your choice (1-3): ");

        string inputChoice = Console.ReadLine().Trim();

        if (inputChoice == "3") {
          Console.WriteLine(" Program terminated. ");
          return;
        }

        if (inputChoice != "1" && inputChoice != "2") {
          Console.WriteLine(" Invalid choice. Please select 1, 2, or 3. ");
          continue;
        }

        try {
          Console.Write(" Enter matrix size (n): ");
          if (!int.TryParse(Console.ReadLine().Trim(), out int size) || size <= 0) {
            Console.WriteLine(" Invalid size. Using default size n = 2 ");
            size = 2;
          }

          SquareMatrix matrixA;
          SquareMatrix matrixB;

          if (inputChoice == "1") {
            Console.WriteLine(" \nCREATING MATRICES WITH RANDOM NUMBERS: ");
            matrixA = new SquareMatrix(size, true);
            matrixB = new SquareMatrix(size, true);
            Console.WriteLine(" Random matrices created successfully. ");
          }
          else {
            Console.WriteLine(" \nMANUAL INPUT OF MATRICES: ");
            Console.WriteLine(" Matrix A: ");
            matrixA = SquareMatrix.ReadFromConsole(size);
            Console.WriteLine(" \nMatrix A entered successfully. ");

            Console.WriteLine(" \nMatrix B: ");
            matrixB = SquareMatrix.ReadFromConsole(size);
            Console.WriteLine(" \nMatrix B entered successfully. ");
          }

          while (true) {
            Console.WriteLine(" \n=== OPERATION MENU === ");
            Console.WriteLine(" 1. Binary operations (+, *) ");
            Console.WriteLine(" 2. Comparison operations (==, !=, >, <, >=, <=) ");
            Console.WriteLine(" 3. Boolean operations ");
            Console.WriteLine(" 4. Determinant calculation ");
            Console.WriteLine(" 5. Inverse matrix ");
            Console.WriteLine(" 6. Type conversion ");
            Console.WriteLine(" 7. Equals, GetHashCode, ToString methods ");
            Console.WriteLine(" 8. IComparable.CompareTo ");
            Console.WriteLine(" 9. Prototype pattern ");
            Console.WriteLine(" 10. Iterate elements ");
            Console.WriteLine(" 11. Exception handling demonstration ");
            Console.WriteLine(" 12. Display matrices ");
            Console.WriteLine(" 13. Return to main menu ");
            Console.WriteLine(" 14. Exit ");
            Console.Write(" Your choice (1-14): ");

            string userChoice = Console.ReadLine().Trim();
            if (userChoice == "14") {
              Console.WriteLine(" Program terminated. ");
              return;
            }
            if (userChoice == "13") break;

            try {
              switch (userChoice) {
                case "1":
                  Console.WriteLine(" \n--- BINARY OPERATIONS --- ");
                  Console.WriteLine(" matrixA + matrixB: ");
                  Console.WriteLine(matrixA + matrixB);
                  Console.WriteLine(" matrixA * matrixB: ");
                  Console.WriteLine(matrixA * matrixB);
                  Console.WriteLine(" matrixA * 2.5: ");
                  Console.WriteLine(matrixA * 2.5);
                  Console.WriteLine(" 2.5 * matrixA: ");
                  Console.WriteLine(2.5 * matrixA);
                  break;

                case "2":
                  Console.WriteLine(" \n--- COMPARISON OPERATIONS --- ");
                  Console.WriteLine($" matrixA == matrixB: {matrixA == matrixB} ");
                  Console.WriteLine($" matrixA != matrixB: {matrixA != matrixB} ");
                  Console.WriteLine($" matrixA > matrixB: {matrixA > matrixB} ");
                  Console.WriteLine($" matrixA < matrixB: {matrixA < matrixB} ");
                  Console.WriteLine($" matrixA >= matrixB: {matrixA >= matrixB} ");
                  Console.WriteLine($" matrixA <= matrixB: {matrixA <= matrixB} ");
                  break;

                case "3":
                  Console.WriteLine(" \n--- BOOLEAN OPERATIONS --- ");
                  if (matrixA)
                    Console.WriteLine(" Matrix A is true (contains non-zero elements) ");
                  else
                    Console.WriteLine(" Matrix A is false (all elements are zero) ");

                  if (matrixB)
                    Console.WriteLine(" Matrix B is true (contains non-zero elements) ");
                  else
                    Console.WriteLine(" Matrix B is false (all elements is zero) ");

                  SquareMatrix zeroMatrix = new SquareMatrix(2);
                  Console.WriteLine(" \nZero matrix test: ");
                  Console.WriteLine(zeroMatrix);
                  if (zeroMatrix)
                    Console.WriteLine(" Zero matrix is true ");
                  else
                    Console.WriteLine(" Zero matrix is false ");
                  break;

                case "4":
                  Console.WriteLine(" \n--- DETERMINANT CALCULATION --- ");
                  Console.WriteLine($" Determinant of A: {matrixA.Determinant:F4} ");
                  Console.WriteLine($" Determinant of B: {matrixB.Determinant:F4} ");
                  break;

                case "5":
                  Console.WriteLine(" \n--- INVERSE MATRIX --- ");
                  try {
                    SquareMatrix inverseA = matrixA.Inverse();
                    Console.WriteLine(" Inverse matrix of A: ");
                    Console.WriteLine(inverseA);
                    Console.WriteLine(" Verification: A * A^-1 (should be identity matrix): ");
                    Console.WriteLine(matrixA * inverseA);
                  }
                  catch (SingularMatrixException exception) {
                    Console.WriteLine($" Error: {exception.Message} ");
                  }
                  break;

                case "6":
                  Console.WriteLine(" \n--- TYPE CONVERSION --- ");
                  string stringRepresentation = matrixA;
                  Console.WriteLine(" Implicit conversion of A to string: ");
                  Console.WriteLine(stringRepresentation);

                  SquareMatrix matrixFromDouble = 5.5;
                  Console.WriteLine(" Implicit conversion double -> SquareMatrix (5.5): ");
                  Console.WriteLine(matrixFromDouble);

                  double determinantValue = (double)matrixA;
                  Console.WriteLine($" Explicit conversion SquareMatrix -> double (determinant): {determinantValue:F4} ");
                  break;

                case "7":
                  Console.WriteLine(" \n--- EQUALS, GETHASHCODE, TOSTRING --- ");
                  Console.WriteLine($" matrixA.Equals(matrixB): {matrixA.Equals(matrixB)} ");
                  Console.WriteLine($" matrixA.GetHashCode(): {matrixA.GetHashCode()} ");
                  Console.WriteLine($" matrixB.GetHashCode(): {matrixB.GetHashCode()} ");
                  Console.WriteLine(" \nmatrixA.ToString(): ");
                  Console.WriteLine(matrixA);
                  break;

                case "8":
                  Console.WriteLine(" \n--- ICOMPARABLE.COMPARETO --- ");
                  int comparisonResult = matrixA.CompareTo(matrixB);
                  if (comparisonResult < 0)
                    Console.WriteLine(" matrixA < matrixB (sum of elements is smaller) ");
                  else if (comparisonResult == 0)
                    Console.WriteLine(" matrixA == matrixB (sum of elements is equal) ");
                  else
                    Console.WriteLine(" matrixA > matrixB (sum of elements is larger) ");
                  break;

                case "9":
                  Console.WriteLine(" \n--- PROTOTYPE PATTERN (DEEP COPY) --- ");
                  SquareMatrix matrixC = matrixA.DeepCopy();
                  Console.WriteLine(" Copy of matrix A (matrix C): ");
                  Console.WriteLine(matrixC);

                  double originalValue = matrixA[0, 0];
                  matrixA[0, 0] = 999.99;
                  Console.WriteLine(" After changing matrixA[0,0] = 999.99: ");
                  Console.WriteLine(" Matrix A (modified): ");
                  Console.WriteLine(matrixA);
                  Console.WriteLine(" Matrix C (copy unchanged - deep copying works): ");
                  Console.WriteLine(matrixC);

                  matrixA[0, 0] = originalValue;
                  break;

                case "10":
                  Console.WriteLine(" \n--- ITERATING ELEMENTS (IENUMERABLE) --- ");
                  Console.Write(" Elements of matrix A: ");
                  foreach (double element in matrixA) {
                    Console.Write($"{element:F2} ");
                  }
                  Console.WriteLine();
                  break;

                case "11":
                  Console.WriteLine(" \n--- EXCEPTION HANDLING DEMONSTRATION --- ");
                  try {
                    Console.WriteLine(" Attempting to create a matrix of size 0... ");
                    SquareMatrix invalidMatrix = new SquareMatrix(0);
                  }
                  catch (InvalidMatrixSizeException exception) {
                    Console.WriteLine($" Exception caught: {exception.Message} ");
                  }

                  try {
                    SquareMatrix matrix2x2 = new SquareMatrix(2, true);
                    SquareMatrix matrix3x3 = new SquareMatrix(3, true);
                    Console.WriteLine(" Attempting to add matrices of different sizes (2x2 and 3x3)... ");
                    SquareMatrix resultMatrix = matrix2x2 + matrix3x3;
                  }
                  catch (MatrixDimensionMismatchException exception) {
                    Console.WriteLine($" Exception caught: {exception.Message} ");
                  }
                  finally {
                    Console.WriteLine(" Finally block executed (resource cleanup demonstration) ");
                  }
                  break;

                case "12":
                  Console.WriteLine(" \n--- CURRENT MATRICES --- ");
                  Console.WriteLine(" Matrix A: ");
                  Console.WriteLine(matrixA);
                  Console.WriteLine(" Matrix B: ");
                  Console.WriteLine(matrixB);
                  break;

                default:
                  Console.WriteLine(" Invalid choice. Please select a number from 1 to 14. ");
                  break;
              }
            }
            catch (MatrixException exception) {
              Console.WriteLine($" Matrix operation error: {exception.Message} ");
            }
            catch (Exception exception) {
              Console.WriteLine($" General error: {exception.Message} ");
            }
          }
        }
        catch (Exception exception) {
          Console.WriteLine($" Error: {exception.Message} ");
        }
      }
    }
  }
}
