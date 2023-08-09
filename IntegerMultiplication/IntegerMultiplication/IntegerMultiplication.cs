using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;

using System.Threading.Tasks;
namespace Problem
{   /* وان ليس للانسان الا ما سعي */
    // *****************************************
    // DON'T CHANGE CLASS OR FUNCTION NAME
    // YOU CAN ADD FUNCTIONS IF YOU NEED TO
    // *****************************************
    public static class IntegerMultiplication
    {
        #region YOUR CODE IS HERE
        //Your Code is Here:
        //==================
        /// <summary>
        /// Multiply 2 large integers of N digits in an efficient way [Karatsuba's Method]
        /// </summary>
        /// <param name="X">First large integer of N digits [0: least significant digit, N-1: most signif. dig.]</param>
        /// <param name="Y">Second large integer of N digits [0: least significant digit, N-1: most signif. dig.]</param>
        /// <param name="N">Number of digits (power of 2)</param>
        /// <returns>Resulting large integer of 2xN digits (left padded with 0's if necessarily) [0: least signif., 2xN-1: most signif.]</returns>
        static public byte[] IntegerMultiply(byte[] X, byte[] Y, int N)
        {
            // Check if X or Y is larger, resize the smaller array to the same size as the larger one
            if (X.Length > Y.Length)
            {
                Array.Resize(ref Y, X.Length);
            }
            else
            {
                Array.Resize(ref X, Y.Length);
            }

            // Check if the length of X or Y is odd, if so, add an extra digit to make it even
            if (X.Length % 2 != 0)
            {
                Array.Resize(ref Y, Y.Length + 1);
                Array.Resize(ref X, X.Length + 1);
            }

            // Set N to the length of X, which is now even
            N = X.Length;

            // If the length of X is small enough, use a simple multiplication algorithm and return the result
            if (N <= 64)
            {
                return Mult(X, Y, N);
            }

            // Split X and Y into left and right halves
            byte[] Xl = X.Take(N / 2).ToArray();
            byte[] Xr = X.Skip(N / 2).ToArray();
            byte[] Yl = Y.Take(N / 2).ToArray();
            byte[] Yr = Y.Skip(N / 2).ToArray();

            // Recursively compute p1, p2, and p3 in parallel using TPL
            Task<byte[]> t1 = Task.Run(() => IntegerMultiply(Xl, Yl, N / 2));
            Task<byte[]> t2 = Task.Run(() => IntegerMultiply(Xr, Yr, N / 2));
            Task<byte[]> t3 = Task.Run(() =>
            {
                byte[] a1 = Add(Xl, Xr);
                byte[] a2 = Add(Yl, Yr);
                return IntegerMultiply(a1, a2, Math.Max(a1.Length, a2.Length));
            });

            // Wait for all three tasks to complete
            Task.WaitAll(t1, t2, t3);

            // Compute the final result using the formula (p1 * 10 ^ n) + ((p2 - p1 - p3) * 10 ^ n / 2) + p3
            byte[] p1 = t1.Result;
            byte[] p3 = t2.Result;
            byte[] p2 = t3.Result;

            byte[] finRes = Subtract(Subtract(p2, p1), p3);
            byte[] newByteArray = new byte[p3.Length + N];
            Array.Copy(p3, 0, newByteArray, N, p3.Length);
            byte[] newByteArray2 = new byte[finRes.Length + N / 2];
            Array.Copy(finRes, 0, newByteArray2, N / 2, finRes.Length);
            byte[] ans = Add(Add(p1, newByteArray), newByteArray2);

            // Resize the answer to twice the length of X and return it
            Array.Resize(ref ans, N * 2);
            return ans;
        }

        static public byte[] Mult(byte[] X, byte[] Y, int N)
        {
            // Create an array to store the result of the multiplication, which has twice the size of the input arrays
            byte[] result = new byte[N * 2];

            // Loop over the digits of X
            for (int i = 0; i < N; i++)
            {
                // Initialize a carry variable to 0 for each digit of X
                byte carry = 0;

                // Loop over the digits of Y
                for (int j = 0; j < N; j++)
                {
                    // Compute the product of the i-th digit of X and the j-th digit of Y, and add the carry and the current result digit
                    int k = i + j;
                    byte prod = (byte)(X[i] * Y[j] + carry + result[k]);

                    // Compute the new carry and the new result digit
                    carry = (byte)(prod / 10);
                    result[k] = (byte)(prod % 10);
                }

                // Store the final carry in the result array at position i + N
                result[i + N] = carry;
            }

            // Return the result array
            return result;
        }

        // This function multiplies the given byte array by 10^n by adding n zeros to the beginning of the array
        public static void Mult10(ref byte[] byteArray, int n)
        {
            byte[] newByteArray = new byte[byteArray.Length + n];
            Array.Copy(byteArray, 0, newByteArray, n, byteArray.Length);
            byteArray = newByteArray;// Update 
        }

        public static byte[] Subtract(byte[] minuend, byte[] subtrahend)
        {
            // Check which array is longer and resize the shorter one to match the length of the longer one
            if (minuend.Length > subtrahend.Length)
            {
                Array.Resize(ref subtrahend, minuend.Length);
            }
            else
            {
                Array.Resize(ref minuend, subtrahend.Length);
            }

            // Get the length of the arrays
            int maxLength = minuend.Length;

            // Initialize variables to track borrow and difference
            int borrow = 0, difference;

            // Create an array to store the result
            byte[] result = new byte[maxLength];

            // Loop through each digit of the arrays
            for (int i = 0; i < maxLength; i++)
            {
                // Get the digits at the current position
                int digit1, digit2;
                digit1 = minuend[i];
                digit2 = subtrahend[i];

                // If there was a borrow from the previous digit, subtract 1 from digit1
                if (borrow == 1)
                {
                    digit1 -= 1;
                }

                // If digit1 is greater than or equal to digit2, subtract normally and set borrow to 0
                if (digit1 >= digit2)
                {
                    difference = digit1 - digit2;
                    borrow = 0;
                }
                // If digit1 is less than digit2, add 10 to digit1 to borrow and subtract normally
                else
                {
                    digit1 += 10;
                    difference = digit1 - digit2;
                    borrow = 1;
                }

                // Store the difference in the result array
                result[i] = (byte)difference;
            }

            // Return the result array
            return result;
        }

        public static byte[] Add(byte[] x, byte[] y)
        {

            // Check which array is longer and resize the shorter one to match the length of the longer one
            if (x.Length > y.Length)
            {
                Array.Resize(ref y, x.Length);
            }
            else
            {
                Array.Resize(ref x, y.Length);
            }

            // Get the length of the arrays
            int maxLength = Math.Max(x.Length, y.Length);

            // Create an array to store the result
            byte[] result = new byte[maxLength];

            // Initialize variable to track carry
            int carry = 0;

            // Loop through each digit of the arrays
            
            for (int i = 0; i < maxLength; i++)
            {
                // Calculate the sum of the digits and carry
                int sum = x[i] + y[i] + carry;

                // Store the result digit in the result array
                result[i] = (byte)(sum % 10);

                // Calculate the carry for the next iteration
                carry = sum / 10;
            }

            // If there is a carry after the last iteration, resize the result array and add the carry as the last digit
            if (carry > 0)
            {
                Array.Resize(ref result, maxLength + 1);
                result[maxLength] = (byte)carry;
            }

            // Return the result array
            return result;

        }

        #endregion
    }
}



