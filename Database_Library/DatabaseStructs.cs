// <summary> Structs for use in simplifying grouped data (i.e. Address, Phone Numbers, Lat/Long). </summary>
namespace Database
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary> Holds Address data in a way that allows it to be easily compared and formatted.</summary>
    public struct Address
    {
        /// <summary> Private field for the state, to allow error checking.</summary>
        private string state;

        /// <summary> Private field for the zip, to allow error checking.</summary>
        private string zip;
        
        /// <summary>Gets or sets the Street Address. No abbreviations.</summary>
        /// <value>The Street Address.</value>
        public string Street { get; set; }

        /// <summary>Gets or sets the City Name.</summary>
        /// <value>The City Name.</value>
        public string City { get; set; }

        /// <summary>Gets or sets the State Initials. Two Letters Only.</summary>
        /// <value>The State Initials.</value>
        public string State 
        {
            get
            {
                return this.state;
            }

            set
            {
                // Make sure we were given two letters and make them uppercase if they aren't already. 
                // Throw an exception if there are more/less than 2 chars.
                if (value.Length != 2)
                {
                    throw new ArgumentException("Address.State must be exactly a two Char string.");
                }

                // Make sure that the two letters we were given appear in the list of recognized state abbreviations.
                string[] abbvs = 
                                {
                                  "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA", "HI", "ID", "IL", "IN", 
                                  "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", 
                                  "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", 
                                  "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "AS", "DC", "FM", "GU", "MH", "MP", 
                                  "PW", "PR", "VI"
                                 };
                if (!abbvs.Any(value.ToUpper().Contains))
                {
                    throw new ArgumentException("Address.State must be a valid Abbreviation");
                }

                this.state = value;
            }
        }

        /// <summary>Gets or sets the State Initials. Must be Five Digits.</summary>
        /// <value>The string representation of the 5 digit zip code.</value>
        public string Zip 
        {
            get
            {
                return this.zip;
            }

            set
            {
                // Make sure we have a 5 char string before we go anywhere else.
                if (value.Length != 5)
                {
                    throw new ArgumentException("Address.Zip must be exactly 5 Chars in length.");
                }

                int i; // Temporary variable for testing that the value is numeric.
                if (!int.TryParse(value, out i))
                {
                    throw new ArgumentException("Address.Zip must be a numeric string.");
                }

                this.zip = value;
            }
        }

        /// <summary>
        /// Operator to check if two address are identical.
        /// </summary>
        /// <param name="a1">The First Address.</param>
        /// <param name="a2">The Second Address.</param>
        /// <returns>True if all fields are identical.</returns>
        public static bool operator ==(Address a1, Address a2)
        {
            return a1.Equals(a2);
        }

        /// <summary>
        /// Operator to check it two address are not identical.
        /// </summary>
        /// <param name="a1">The First Address.</param>
        /// <param name="a2">The Second Address.</param>
        /// <returns>True if any fields do not match.</returns>
        public static bool operator !=(Address a1, Address a2)
        {
            return !a1.Equals(a2);
        }

        /// <summary>
        /// Converts the specified Address to the properly formatted version.
        /// </summary>
        /// <returns>Street, City, ST 00000.</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}, {2} {3}", this.Street, this.City, this.State, this.Zip);
        }

        /// <summary>
        /// Override method to facilitate the operators above. Can also be used in it's own right.
        /// </summary>
        /// <param name="obj">An address struct to check.</param>
        /// <returns>True if the given address is equal to the caller.</returns>
        public override bool Equals(object obj)
        {
            // If the parameter is empty, return false right away.
            if (obj == null)
            {
                return false;
            }

            // Check if the object can be cast to an address. If not, return false.
            Address a = (Address)obj;
            if ((object)a == null)
            {
                return false;
            }

            return (this.Street == a.Street) &&
                    (this.City  == a.City) &&
                    (this.State == a.State) &&
                    (this.Zip == a.Zip);
        }

        /// <summary>
        /// Important in the event the item is ever used as a Dictionary Key, HashSet, etc. This is used in the absence
        /// of a custom IEqualityComparer to group items into buckets. If the hash codes for two objects do not match, 
        /// they are never considered equal. If they do, then <see cref="Equals"/> will be called to see if they truly match.
        /// </summary>
        /// <returns>The Object's Hash Code.</returns>
        public override int GetHashCode()
        {
            int i = 0;
            i += this.Street.GetHashCode();
            i += this.City.GetHashCode();
            i += this.State.GetHashCode();
            i += this.Zip.GetHashCode();
            return i;
        }
    }

    /// <summary>
    /// Holds Geographic Coordinates, and allows various operations to be performed on them.
    /// </summary>
    public struct GeographicCoordinates
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeographicCoordinates"/> struct.
        /// </summary>
        /// <param name="latitude">The Latitude of a location.</param>
        /// <param name="longitude">The Longitude of a location.</param>
        public GeographicCoordinates(float latitude, float longitude) 
            : this()
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeographicCoordinates"/> struct.
        /// </summary>
        /// <param name="latitude">The Latitude of a location.</param>
        /// <param name="longitude">The Longitude of a location.</param>
        public GeographicCoordinates(string latitude, string longitude)
            : this()
        {
            float lat, lng;
            if (!float.TryParse(latitude, out lat) || !float.TryParse(longitude, out lng))
            {
                throw new InvalidCastException(string.Format("Could not cast to a valid double ({0}, {1})", latitude, longitude));
            }

            this.Latitude = lat;
            this.Longitude = lng;
        }

        /// <summary> Gets or sets the Latitude of a location.</summary>
        /// <value> A floating point value for the Latitude.</value>
        public float Latitude { get; set; }

        /// <summary> Gets or sets the Longitude of a location.</summary>
        /// /// <value> A floating point value for the Longitude.</value>
        public float Longitude { get; set; }

        /// <summary>
        /// Override method to determine if two sets of Coordinates are equal.
        /// </summary>
        /// <param name="c1">The first coordinate.</param>
        /// <param name="c2">The second coordinate.</param>
        /// <returns>True, if both pairs of coordinates are identical.</returns>
        public static bool operator ==(GeographicCoordinates c1, GeographicCoordinates c2)
        {
            return c1.Equals(c2);
        }

        /// <summary>
        /// Override method to determine if two sets of Coordinates are not equal.
        /// </summary>
        /// <param name="c1">The first coordinate.</param>
        /// <param name="c2">The second coordinate.</param>
        /// <returns>True, if both any part of the coordinates are not identical.</returns>
        public static bool operator !=(GeographicCoordinates c1, GeographicCoordinates c2)
        {
            return !c1.Equals(c2);
        }

        /// <summary>
        /// Override method to facilitate the operators above. Can also be used in it's own right.
        /// </summary>
        /// <param name="obj">A GeographicCoordinate struct to check.</param>
        /// <returns>True if the given Coordinate is equal to the caller.</returns>
        public override bool Equals(object obj)
        {
            // If the object is empty, return false            
            if (obj == null)
            {
                return false;
            }

            // If the object cannot be cast to the correct type, return false
            GeographicCoordinates coords = (GeographicCoordinates)obj;
            if ((object)coords == null)
            {
                return false;
            }

            return (this.Latitude == coords.Latitude) && (this.Longitude == coords.Longitude);
        }

        /// <summary>
        /// See the descriptor for Address's GetHashCode() for an explanation.
        /// </summary>
        /// <returns>The Object's HashCode.</returns>
        public override int GetHashCode()
        {
            return this.Latitude.GetHashCode() + this.Longitude.GetHashCode();
        }

        /// <summary>
        /// Converts the stored coordinates to a properly formatted string.
        /// </summary>
        /// <returns>A string in the form of "(Latitude), (Longitude)".</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Latitude.ToString(), this.Longitude.ToString());
        }

        /// <summary>
        /// Determines the distance in miles between the current GeographicCoordinate and a given one. 
        /// </summary>
        /// <param name="location2">The GeographicCoordinate to measure from.</param>
        /// <param name="precision">The number of decimal places requested for the answer (defaults to 2).</param>
        /// <returns>A string with the calculated answer in miles.</returns>
        public string GetDistanceFrom(GeographicCoordinates location2, int precision = 2)
        {
            double r = 3959; // Average radius of the Earth in miles.
            double difLat = Radians(location2.Latitude - this.Latitude);
            double difLong = Radians(location2.Longitude - this.Longitude);

            double a = (Math.Sin(difLat / 2) * Math.Sin(difLat / 2)) +
                       (Math.Cos(Radians(this.Latitude)) *
                       Math.Cos(Radians(location2.Latitude)) *
                       (Math.Sin(difLong / 2) * Math.Sin(difLong / 2)));

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return string.Format("{0} mi.", Math.Round(r * c, precision).ToString());
        }

        /// <summary>
        /// Private method to convert degrees to radians.
        /// </summary>
        /// <param name="degrees">The double to Convert to radians.</param>
        /// <returns>The give degrees as a double in radians.</returns>
        private static double Radians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }

    /// <summary>
    /// Tiny Struct to make it clearer when something is provided in american currency.
    /// </summary>
    public struct Money
    {
        /// <summary>The value, as a simple decimal, of the currency.</summary>
        private decimal value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Money"/> struct.
        /// </summary>
        /// <param name="value">The value to be stored in the struct.</param>
        public Money(decimal value)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Money"/> struct.
        /// </summary>
        /// <param name="value">The value to be stored in the struct.</param>
        public Money(string value)
        {
            decimal.TryParse(value, out this.value);
        }

        /// <summary>Gets or sets the value of the currency.</summary>
        /// <value>The value.</value>
        public decimal Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
            }
        }

        /// <summary>
        /// Returns the value of the struct as an american currency formatted string (e.g. $1,345.90).
        /// </summary>
        /// <returns>The value of the struct as a formatted string.</returns>
        public override string ToString()
        {
            return this.value.ToString("C");
        }
    }

    /// <summary>
    /// Struct for handling and parsing telephone numbers.
    /// </summary>
    public struct TelephoneNumber
    {
        /// <summary> Integer array that holds the three parts of the telephone number. </summary>
        private int[] number;

        /// <summary> Integer that represents the extension of the telephone number. </summary>
        private int? extension;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelephoneNumber"/> struct and parses a string to convert it to a valid phone number.
        /// </summary>
        /// <param name="phonenumber">A string representation of a phone number. May contain other characters as long as it has 10 digits (e.g. "410-290-8913" or "3015962964").</param>
        public TelephoneNumber(string phonenumber)
            : this(phonenumber, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelephoneNumber"/> struct and parses a string to convert it to a valid phone number. A separate string is provided for an extension.
        /// </summary>
        /// <param name="phonenumber">A string representation of a phone number. May contain other characters as long as it has 10 digits (e.g. "410-290-8913" or "3015962964").</param>
        /// <param name="extension">The extension for the phone number. Must be able to be parsed to an integer.</param>
        public TelephoneNumber(string phonenumber, string extension)
        {
            // Replace any non-numeric parts of the string with blanks.
            string tele_number = Regex.Replace(phonenumber, "[^0-9]", string.Empty);
            this.number = new int[3];
            this.extension = null;
            
            // If the string passed was empty, we're not going to bother with any of this. The struct will remain empty.
            if (tele_number != string.Empty)
            {
                // Make sure we've got something we can turn into a phone number.
                if (
                    (tele_number.Length != 10)
                    ||
                    (
                    !int.TryParse(tele_number.Substring(0, 3), out this.number[0]) ||
                    !int.TryParse(tele_number.Substring(3, 3), out this.number[1]) ||
                    !int.TryParse(tele_number.Substring(6), out this.number[2])
                    )
                   )
                {
                    throw new ArgumentException(string.Format("'phonenumber' must be a valid 10-digit phone number."));
                }

                string ext = Regex.Replace(extension, "[^0-9]", string.Empty);

                if (ext != string.Empty)
                {
                    // Make sure we haven't somehow got some crazy extension that can't be made an integer.
                    int int_test;
                    if (!int.TryParse(extension, out int_test))
                    {
                        throw new ArgumentException("'extension' must be a valid collection of digits");
                    }

                    this.extension = int_test;
                }
            }
        }

        /// <summary> Gets the string representation of the number portion of the Telephone Number. </summary>
        /// <value> The number stored in this struct. </value>
        public string Number
        {
            get
            {
                if (this.number == null)
                {
                    return string.Empty;
                }

                // When we initialize the number struct, they are filled to read {0, 0, 0} - we need to return null if this is the case at this point.
                if (this.number[0] == 0 && this.number[1] == 0 && this.number[2] == 0)
                {
                    return string.Empty;
                }

                return this.number[0].ToString() + this.number[1].ToString() + this.number[2].ToString();
            }
        }

        /// <summary> Gets the string representation of the extension portion of the Telephone Number. </summary>
        /// <value> The extension stored in this struct. </value>
        public string Ext
        {
            get
            {
                return this.extension.ToString();
            }
        }

        /// <summary>
        /// Override method to determine if two Telephone Numbers are identical.
        /// </summary>
        /// <param name="t1">The first telephone number.</param>
        /// <param name="t2">The second telephone number.</param>
        /// <returns>True, if both numbers are equal.</returns>
        public static bool operator ==(TelephoneNumber t1, TelephoneNumber t2)
        {
            return t1.Equals(t2);
        }

        /// <summary>
        /// Override method to determine if two Telephone Numbers are different.
        /// </summary>
        /// <param name="t1">The first telephone number.</param>
        /// <param name="t2">The second telephone number.</param>
        /// <returns>True, if both numbers are different.</returns>
        public static bool operator !=(TelephoneNumber t1, TelephoneNumber t2)
        {
            return !t1.Equals(t2);
        }

        /// <summary>
        /// Method override to create a string representation of this struct.
        /// </summary>
        /// <returns>A properly formatted string.</returns>
        public override string ToString()
        {
            if (this.number[0] + this.number[1] + this.number[2] == 0)
            {
                return string.Empty;
            }
            
            if (this.extension == null)
            {
                return string.Format("{0}-{1}-{2}", this.number[0], this.number[1], this.number[2]);
            }
            else
            {
                return string.Format("{0}-{1}-{2}x{3}", this.number[0], this.number[1], this.number[2], this.extension);
            }
        }

        /// <summary>
        /// Override method to facilitate operators above. May be used in it's own right if desired.
        /// </summary>
        /// <param name="obj">A Telephone number struct to check.</param>
        /// <returns>True, if the object is equal to the current struct.</returns>
        public override bool Equals(object obj)
        {
            // If the object is empty, return false.
            if (obj == null)
            {
                return false;
            }

            // If the object cannot be cast to the correct type, return false
            TelephoneNumber phone_number = (TelephoneNumber)obj;
            if ((object)phone_number == null)
            {
                return false;
            }

            return (this.number == phone_number.number) && (this.extension == phone_number.extension);
        }

        /// <summary>
        /// Gets the object's hash code. Refer to the same method above in Address for an explanation.
        /// </summary>
        /// <returns>The object's hash code.</returns>
        public override int GetHashCode()
        {
            return this.number.GetHashCode() + this.extension.GetHashCode();
        }
    }
}