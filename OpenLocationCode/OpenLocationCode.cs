// Copyright 2014 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Diagnostics;
using System.Text;

namespace com.google.openlocationcode
{
    /// <summary>
    /// Convert locations to and from convenient short codes.
    ///
    /// Open Location Codes are short, ~10 character codes that can be used instead of street
    /// addresses. The codes can be generated and decoded offline, and use a reduced character set that
    /// minimises the chance of codes including words.
    ///
    /// This provides both object and static methods.
    ///
    /// Create an object with:
    /// OpenLocationCode code = new OpenLocationCode("7JVW52GR+2V");
    /// OpenLocationCode code = new OpenLocationCode("52GR+2V");
    /// OpenLocationCode code = new OpenLocationCode(27.175063, 78.042188);
    /// OpenLocationCode code = new OpenLocationCode(27.175063, 78.042188, 11);
    ///
    /// Once you have a code object, you can apply the other methods to it, such as to shorten:
    /// code.shorten(27.176, 78.05)
    ///
    /// Recover the nearest match (if the code was a short code):
    /// code.recover(27.176, 78.05)
    ///
    /// Or decode a code into it's coordinates, returning a CodeArea object.
    /// code.decode()
    ///
    /// @author Jiri Semecky
    /// @author Doug Rinckes
    /// 
    /// This C# .NET Port by Michael Coyle, Blue Toque Software
    /// </summary>
    public sealed class OpenLocationCode
    {

        /// <summary>
        /// Creates Open Location Code object for the provided code.
        /// </summary>
        /// <param name="code">code A valid OLC code.Can be a full code or a shortened code</param>
        public OpenLocationCode(string code)
        {
            if (!IsValidCode(code.ToUpper()))
                throw new ArgumentException("The provided code '" + code + "' is not a valid Open Location Code.");
            this.m_code = code.ToUpper();
        }

        /// <summary>
        /// Creates Open Location Code.
        /// throws ArgumentException if the code length is not valid.
        /// </summary>
        /// <param name="latitude">latitude The latitude in decimal degrees.</param>
        /// <param name="longitude">longitude The longitude in decimal degrees.</param>
        /// <param name="codeLength">codeLength The desired number of digits in the code.</param>
        public OpenLocationCode(double latitude, double longitude, int codeLength)
        {
            // Check that the code length requested is valid.
            if (codeLength < 4 || (codeLength < PAIR_CODE_LENGTH && codeLength % 2 == 1))
                throw new ArgumentException("Illegal code length " + codeLength);

            // Ensure that latitude and longitude are valid.
            latitude = ClipLatitude(latitude);
            longitude = NormalizeLongitude(longitude);

            // Latitude 90 needs to be adjusted to be just less, so the returned code can also be decoded.
            if (latitude == (double)LATITUDE_MAX)
                latitude = latitude - 0.9 * ComputeLatitudePrecision(codeLength);

            // Adjust latitude and longitude to be in positive number ranges.
            Decimal remainingLatitude = new Decimal(latitude) + LATITUDE_MAX;
            Decimal remainingLongitude = new Decimal(longitude) + LONGITUDE_MAX;

            // Count how many digits have been created.
            int generatedDigits = 0;

            // Store the code.
            StringBuilder codeBuilder = new StringBuilder();
            
            // The precisions are initially set to ENCODING_BASE^2 because they will be immediately
            // divided.
            Decimal latPrecision = ENCODING_BASE * ENCODING_BASE;
            Decimal lngPrecision = ENCODING_BASE * ENCODING_BASE;
            while (generatedDigits < codeLength)
            {
                if (generatedDigits < PAIR_CODE_LENGTH)
                {
                    // Use the normal algorithm for the first set of digits.
                    latPrecision = latPrecision / ENCODING_BASE;
                    lngPrecision = lngPrecision / ENCODING_BASE;
                    Decimal latDigit = Math.Floor(remainingLatitude / latPrecision);
                    Decimal lngDigit = Math.Floor(remainingLongitude / lngPrecision);
                    remainingLatitude = remainingLatitude - (latPrecision * latDigit);
                    remainingLongitude = remainingLongitude - (lngPrecision * lngDigit);
                    codeBuilder.Append(CODE_ALPHABET[(int)latDigit]);
                    codeBuilder.Append(CODE_ALPHABET[(int)lngDigit]);
                    generatedDigits += 2;
                }
                else
                {
                    // Use the 4x5 grid for remaining digits.
                    latPrecision = latPrecision / GRID_ROWS;
                    lngPrecision = lngPrecision / GRID_COLUMNS;
                    Decimal row = Math.Floor(remainingLatitude / latPrecision);
                    Decimal col = Math.Floor(remainingLongitude / lngPrecision);
                    remainingLatitude = remainingLatitude - (latPrecision * row);
                    remainingLongitude = remainingLongitude - (lngPrecision * col);
                    codeBuilder.Append(CODE_ALPHABET[(int)row * (int)GRID_COLUMNS + (int)col]);
                    generatedDigits += 1;
                }

                // If we are at the separator position, add the separator.
                if (generatedDigits == SEPARATOR_POSITION)
                    codeBuilder.Append(SEPARATOR);
            }
            // If the generated code is shorter than the separator position, pad the code and add the
            // separator.
            if (generatedDigits < SEPARATOR_POSITION)
            {
                for (; generatedDigits < SEPARATOR_POSITION; generatedDigits++)
                    codeBuilder.Append(PADDING_CHARACTER);
                codeBuilder.Append(SEPARATOR);
            }

            this.m_code = codeBuilder.ToString();
        }

        /// <summary>
        ///  Creates Open Location Code with the default precision length.
        /// </summary>
        /// <param name="latitude">The latitude in decimal degrees</param>
        /// <param name="longitude">The longitude in decimal degrees</param>
        public OpenLocationCode(double latitude, double longitude) :
            this(latitude, longitude, CODE_PRECISION_NORMAL)
        {
        }

        /// <summary>
        /// Provides a normal precision code, approximately 14x14 meters.
        /// </summary>
        public static int CODE_PRECISION_NORMAL = 10;

        /// <summary>
        /// Provides an extra precision code, approximately 2x3 meters.
        /// </summary>
        public static int CODE_PRECISION_EXTRA = 11;

        /// <summary>
        /// A separator used to break the code into two parts to aid memorability.
        /// </summary>
        private static char SEPARATOR = '+';

        /// <summary>
        /// The number of characters to place before the separator.
        /// </summary>
        private static int SEPARATOR_POSITION = 8;

        /// <summary>
        /// The character used to pad codes.
        /// </summary>
        private static char PADDING_CHARACTER = '0';

        /// <summary>
        /// The character set used to encode the values.
        /// </summary>
        private static string CODE_ALPHABET = "23456789CFGHJMPQRVWX";

        // Note: The double type can't be used because of the rounding arithmetic due to floating point
        // implementation. Eg. "8.95 - 8" can give result 0.9499999999999 instead of 0.95 which
        // incorrectly classify the points on the border of a cell. Therefore all the calcuation is done
        // using Decimal.

        /// <summary>
        /// The base to use to convert numbers to/from.
        /// </summary>
        private static Decimal ENCODING_BASE = new Decimal(CODE_ALPHABET.Length);

        /// <summary>
        /// The maximum value for latitude in degrees.
        /// </summary>
        private static Decimal LATITUDE_MAX = new Decimal(90);

        /// <summary>
        /// The maximum value for longitude in degrees.
        /// </summary>
        private static Decimal LONGITUDE_MAX = new Decimal(180);

        /// <summary>
        /// Maxiumum code length using just lat/lng pair encoding.
        /// </summary>
        private static int PAIR_CODE_LENGTH = 10;

        /// <summary>
        /// Number of columns in the grid refinement method.
        /// </summary>
        private static Decimal GRID_COLUMNS = new Decimal(4);

        /// <summary>
        /// Number of rows in the grid refinement method.
        /// </summary>
        private static Decimal GRID_ROWS = new Decimal(5);

        /// <summary>
        ///Coordinates of a decoded Open Location Code.
        ///<p>The coordinates include the latitude and longitude of the lower 
        ///left and upper right corners and the center of the bounding box for 
        ///the area the code represents.
        /// </summary>
        public class CodeArea
        {

            private Decimal m_southLatitude;
            private Decimal m_westLongitude;
            private Decimal m_northLatitude;
            private Decimal m_eastLongitude;

            public CodeArea(Decimal southLatitude, Decimal westLongitude, Decimal northLatitude, Decimal eastLongitude)
            {
                this.m_southLatitude = southLatitude;
                this.m_westLongitude = westLongitude;
                this.m_northLatitude = northLatitude;
                this.m_eastLongitude = eastLongitude;
            }

            public double SouthLatitude { get { return (double)m_southLatitude; } }

            public double WestLongitude { get { return (double)m_westLongitude; } }

            public double LatitudeHeight { get { return (double)(m_northLatitude - m_southLatitude); } }

            public double LongitudeWidth { get { return (double)(m_eastLongitude - m_westLongitude); } }

            public double CenterLatitude { get { return (double)((m_southLatitude + m_northLatitude) / 2); } }

            public double CenterLongitude { get { return (double)((m_westLongitude + m_eastLongitude) / 2); } }

            public double NorthLatitude { get { return (double)m_northLatitude; } }

            public double EastLongitude { get { return (double)m_eastLongitude; } }
        }

        /// <summary>
        /// The current code for objects.
        /// </summary>
        private string m_code;

        /// <summary>
        /// Returns the string representation of the code.
        /// </summary>
        public string Code { get { return m_code; } }

        /// <summary>
        ///  Decodes <see cref="OpenLocationCode"/> object into  <see cref="CodeArea"/> object encapsulating
        ///  latitude/longitude bounding box.
        /// </summary>
        /// <returns></returns>
        public CodeArea Decode()
        {
            // you need an area code to decode a short code
            if (!IsFullCode(m_code))
                throw new Exception( "Method decode() could only be called on valid full codes, code was " + m_code + ".");

            // Strip padding and separator characters out of the code.
            string decoded = m_code
                .Replace(SEPARATOR.ToString(), "")
                .Replace(PADDING_CHARACTER.ToString(), "");

            int digit = 0;
            
            // The precisions are initially set to ENCODING_BASE^2 because they will be immediately
            // divided.
            Decimal latPrecision = ENCODING_BASE * ENCODING_BASE;
            Decimal lngPrecision = ENCODING_BASE * ENCODING_BASE;
            // Save the coordinates.
            Decimal southLatitude = new Decimal(0);
            Decimal westLongitude = new Decimal(0);

            // Decode the digits.
            while (digit < decoded.Length)
            {
                if (digit < PAIR_CODE_LENGTH)
                {
                    // Decode a pair of digits, the first being latitude and the second being longitude.
                    latPrecision = latPrecision / ENCODING_BASE;
                    lngPrecision = lngPrecision / ENCODING_BASE;
                    int digitVal = CODE_ALPHABET.IndexOf(decoded[digit]);
                    southLatitude = southLatitude + (latPrecision * new Decimal(digitVal));
                    digitVal = CODE_ALPHABET.IndexOf(decoded[digit + 1]);
                    westLongitude = westLongitude + (lngPrecision * new Decimal(digitVal));
                    digit += 2;
                }
                else
                {
                    // Use the 4x5 grid for digits after 10.
                    int digitVal = CODE_ALPHABET.IndexOf(decoded[digit]);
                    int row = (int)(digitVal / (int)GRID_COLUMNS);
                    int col = digitVal % (int)GRID_COLUMNS;
                    latPrecision = latPrecision / (GRID_ROWS);
                    lngPrecision = lngPrecision / (GRID_COLUMNS);
                    southLatitude = southLatitude + (latPrecision * (new Decimal(row)));
                    westLongitude = westLongitude + (lngPrecision * (new Decimal(col)));
                    digit += 1;
                }
            }
            return new CodeArea(
                southLatitude - (LATITUDE_MAX),
                westLongitude - (LONGITUDE_MAX),
                southLatitude - (LATITUDE_MAX) + (latPrecision),
                westLongitude - (LONGITUDE_MAX) + (lngPrecision));
        }

        /// <summary>
        /// Returns whether this {@link OpenLocationCode} is a full Open Location Code.
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return m_code.IndexOf(SEPARATOR) == SEPARATOR_POSITION;
        }

        /// <summary>
        /// Returns whether this <see cref="OpenLocationCode"/> is a short Open Location Code.
        /// </summary>
        /// <returns></returns>
        public bool IsShort()
        {
            return m_code.IndexOf(SEPARATOR) >= 0 && m_code.IndexOf(SEPARATOR) < SEPARATOR_POSITION;
        }

        /// <summary>
        /// Returns whether this <see cref="OpenLocationCode"/> is a padded Open Location Code, meaning that it
        /// contains less than 8 valid digits.
        /// </summary>
        /// <returns></returns>
        private bool IsPadded()
        {
            return m_code.IndexOf(PADDING_CHARACTER) >= 0;
        }

        /// <summary>
        /// Returns short <see cref="OpenLocationCode"/> from the full Open Location Code created by removing
        /// four or six digits, depending on the provided reference point.It removes as many digits as
        /// possible.
        /// </summary>
        /// <param name="referenceLatitude"></param>
        /// <param name="referenceLongitude"></param>
        /// <returns></returns>
        public OpenLocationCode Shorten(double referenceLatitude, double referenceLongitude)
        {
            if (!IsFull())
                throw new Exception("shorten() method could only be called on a full code.");

            if (IsPadded())
                throw new Exception("shorten() method can not be called on a padded code.");

            CodeArea codeArea = Decode();
            double range = Math.Max(
                Math.Abs(referenceLatitude - codeArea.CenterLatitude),
                Math.Abs(referenceLongitude - codeArea.CenterLongitude));

            // We are going to check to see if we can remove three pairs, two pairs or just one pair of
            // digits from the code.
            for (int i = 4; i >= 1; i--)
            {
                // Check if we're close enough to shorten. The range must be less than 1/2
                // the precision to shorten at all, and we want to allow some safety, so
                // use 0.3 instead of 0.5 as a multiplier.
                if (range < (ComputeLatitudePrecision(i * 2) * 0.3))
                {
                    // We're done.
                    return new OpenLocationCode(m_code.Substring(i * 2));
                }
            }
            throw new ArgumentException("Reference location is too far from the Open Location Code center.");
        }

        /// <summary>
        /// Returns an <see cref="OpenLocationCode"/> object representing a full Open Location Code from this
        /// (short) Open Location Code, given the reference location.
        /// </summary>
        /// <param name="referenceLatitude"></param>
        /// <param name="referenceLongitude"></param>
        /// <returns></returns>
        public OpenLocationCode Recover(double referenceLatitude, double referenceLongitude)
        {
            // Note: each code is either full xor short, no other option.
            if (IsFull())
                return this;

            referenceLatitude = ClipLatitude(referenceLatitude);
            referenceLongitude = NormalizeLongitude(referenceLongitude);

            int digitsToRecover = SEPARATOR_POSITION - m_code.IndexOf(SEPARATOR);

            // The precision (height and width) of the missing prefix in degrees.
            double prefixPrecision = Math.Pow((int)ENCODING_BASE, 2 - (digitsToRecover / 2));

            // Use the reference location to generate the prefix.
            string recoveredPrefix =
                new OpenLocationCode(referenceLatitude, referenceLongitude)
                    .Code
                    .Substring(0, digitsToRecover);
          
            // Combine the prefix with the short code and decode it.
            OpenLocationCode recovered = new OpenLocationCode(recoveredPrefix + m_code);
            CodeArea recoveredCodeArea = recovered.Decode();
            
            // Work out whether the new code area is too far from the reference location. If it is, we
            // move it. It can only be out by a single precision step.
            double recoveredLatitude = recoveredCodeArea.CenterLatitude;
            double recoveredLongitude = recoveredCodeArea.CenterLongitude;

            // Move the recovered latitude by one precision up or down if it is too far from the reference,
            // unless doing so would lead to an invalid latitude.
            double latitudeDiff = recoveredLatitude - referenceLatitude;
            if (latitudeDiff > prefixPrecision / 2 && recoveredLatitude - prefixPrecision > -(int)LATITUDE_MAX)
                recoveredLatitude -= prefixPrecision;
            else if (latitudeDiff < -prefixPrecision / 2 && recoveredLatitude + prefixPrecision < (int)LATITUDE_MAX)
                recoveredLatitude += prefixPrecision;

            // Move the recovered longitude by one precision up or down if it is too far from the
            // reference.
            double longitudeDiff = recoveredCodeArea.CenterLongitude - referenceLongitude;
            if (longitudeDiff > prefixPrecision / 2)
                recoveredLongitude -= prefixPrecision;
            else if (longitudeDiff < -prefixPrecision / 2)
                recoveredLongitude += prefixPrecision;

            return new OpenLocationCode(
                recoveredLatitude, recoveredLongitude, recovered.Code.Length - 1);
        }

        /// <summary>
        /// Returns whether the bounding box specified by the Open Location Code contains provided point.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public bool Contains(double latitude, double longitude)
        {
            CodeArea codeArea = Decode();
            return codeArea.SouthLatitude <= latitude
                && latitude < codeArea.NorthLatitude
                && codeArea.WestLongitude <= longitude
                && longitude < codeArea.EastLongitude;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null || this.GetType() != obj.GetType())
                return false;
            OpenLocationCode that = (OpenLocationCode)obj;
            return GetHashCode() == that.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_code != null ? m_code.GetHashCode() : 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Code;
        }

        #region public static methods

        /// <summary>
        /// Returns whether the provided Open Location Code is a full Open Location Code. 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsFull(string code)
        {
            return new OpenLocationCode(code).IsFull();
        }

        /// <summary>
        /// Returns whether the provided Open Location Code is a short Open Location Code. 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsShort(string code)
        {
            return new OpenLocationCode(code).IsShort();
        }

        /// <summary>
        /// Encodes latitude/longitude into 10 digit Open Location Code. This method is equivalent to
        /// creating the OpenLocationCode object and getting the code from it.
        /// </summary>
        /// <param name="latitude">The latitude in decimal degrees.</param>
        /// <param name="longitude">The longitude in decimal degrees</param>
        /// <returns></returns>
        public static string Encode(double latitude, double longitude)
        {
            return new OpenLocationCode(latitude, longitude).Code;
        }

        /// <summary>
        /// Encodes latitude/longitude into Open Location Code of the provided length. This method is
        /// equivalent to creating the OpenLocationCode object and getting the code from it.
        /// </summary>
        /// <param name="latitude">The latitude in decimal degrees.</param>
        /// <param name="longitude">The longitude in decimal degrees</param>
        /// <param name="codeLength"></param>
        /// <returns></returns>
        public static string Encode(double latitude, double longitude, int codeLength)
        {
            return new OpenLocationCode(latitude, longitude, codeLength).Code;
        }

        /// <summary>
        ///  Decodes <see cref="OpenLocationCode"/> object into  <see cref="CodeArea"/> object encapsulating
        ///  latitude/longitude bounding box.
        /// </summary>
        /// <param name="code">Open Location Code to be decoded.</param>
        /// <returns></returns>
        public static CodeArea Decode(string code)
        {
            return new OpenLocationCode(code).Decode();
        }

        /// <summary>
        /// Returns whether the provided Open Location Code is a padded Open Location Code, 
        /// meaning that it contains less than 8 valid digits.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsPadded(string code)
        {
            return new OpenLocationCode(code).IsPadded();
        }

        /// <summary>
        /// Returns whether the provided string is a valid Open Location code. 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsValidCode(string code)
        {
            if (code == null || code.Length < 2)
                return false;

            code = code.ToUpper();

            // There must be exactly one separator.
            int separatorPosition = code.IndexOf(SEPARATOR);
            if (separatorPosition == -1)
                return false;
            if (separatorPosition != code.LastIndexOf(SEPARATOR))
                return false;

            if (separatorPosition % 2 != 0)
                return false;

            // Check first two characters: only some values from the alphabet are permitted.
            if (separatorPosition == 8)
            {
                // First latitude character can only have first 9 values.
                int index0 = CODE_ALPHABET.IndexOf(code[0]);
                if (index0 < 0 || index0 > 8)
                    return false;

                // First longitude character can only have first 18 values.
                int index1 = CODE_ALPHABET.IndexOf(code[1]);
                if (index1 < 0 || index1 > 17)
                    return false;
            }

            // Check the characters before the separator.
            bool paddingStarted = false;

            for (int i = 0; i < separatorPosition; i++)
            {
                if (paddingStarted)
                {
                    // Once padding starts, there must not be anything but padding.
                    if (code[i] != PADDING_CHARACTER)
                        return false;
                    continue;
                }
                if (CODE_ALPHABET.IndexOf(code[i]) != -1)
                    continue;

                if (PADDING_CHARACTER == code[i])
                {
                    paddingStarted = true;
                    // Padding can start on even character: 2, 4 or 6.
                    if (i != 2 && i != 4 && i != 6)
                        return false;
                    continue;
                }
                return false; // Illegal character.
            }

            // Check the characters after the separator.
            if (code.Length > separatorPosition + 1)
            {
                if (paddingStarted)
                    return false;

                // Only one character after separator is forbidden.
                if (code.Length == separatorPosition + 2)
                    return false;

                for (int i = separatorPosition + 1; i < code.Length; i++)
                {
                    if (CODE_ALPHABET.IndexOf(code[i]) == -1)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns if the code is a valid full Open Location Code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsFullCode(string code)
        {
            try
            {
                return new OpenLocationCode(code).IsFull();
            }
            catch (Exception e)
            {
                Trace.TraceError("Error:\r\n{0}", e);
                return false;
            }
        }

        /// <summary>
        /// Returns if the code is a valid short Open Location Code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsShortCode(string code)
        {
            try
            {
                return new OpenLocationCode(code).IsShort();
            }
            catch (Exception e)
            {
                Trace.TraceError("Error:\r\n{0}", e);
                return false;
            }
        }

        #endregion

        #region private static methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <returns></returns>
        private static double ClipLatitude(double latitude)
        {
            return Math.Min(Math.Max(latitude, -(int)LATITUDE_MAX), (int)LATITUDE_MAX);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="longitude"></param>
        /// <returns></returns>
        private static double NormalizeLongitude(double longitude)
        {
            while (longitude < -(int)LONGITUDE_MAX)
            {
                longitude = longitude + (int)LONGITUDE_MAX * 2;
            }
            while (longitude >= (int)LONGITUDE_MAX)
            {
                longitude = longitude - (int)(LONGITUDE_MAX) * 2;
            }
            return longitude;
        }

        /// <summary>
        /// Compute the latitude precision value for a given code length. Lengths <= 10 have the same
        /// precision for latitude and longitude, but lengths > 10 have different precisions due to the
        /// grid method having fewer columns than rows.Copied from the JS implementation.
        /// </summary>
        /// <param name="codeLength"></param>
        /// <returns></returns>
        private static double ComputeLatitudePrecision(int codeLength)
        {
            if (codeLength <= CODE_PRECISION_NORMAL)
            {
                return Math.Pow((int)ENCODING_BASE, Math.Floor((double)codeLength / -2 + 2));
            }
            return Math.Pow((int)ENCODING_BASE, -3)
                / Math.Pow((int)GRID_ROWS, codeLength - PAIR_CODE_LENGTH);
        }

        #endregion
    }
}