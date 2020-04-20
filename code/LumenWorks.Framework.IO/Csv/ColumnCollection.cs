using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace LumenWorks.Framework.IO.Csv
{
    public class ColumnCollection : ICollection<Column>
    {
        private readonly IList<Column> _list = new List<Column>();
        private readonly Dictionary<string, Column> _columnFromName = new Dictionary<string, Column>(); // Links names to columns
        private CultureInfo _culture = CultureInfo.InvariantCulture;
        private bool _cultureUserSet;
        private StringComparer _hashCodeProvider = StringComparer.Create(CultureInfo.InvariantCulture, true);
        private int _defaultNameIndex = 1;

        /// <devdoc>
        ///     <para>
        ///         Gets the <see cref='Column' />
        ///         from the collection at the specified index.
        ///     </para>
        /// </devdoc>
        public Column this[int index]
        {
            get
            {
                try
                {
                    // Perf: use the readonly _list field directly and let ArrayList check the range
                    return _list[index];
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new ArgumentOutOfRangeException("index", "Column index is out of range");
                }
            }
        }

        /// <devdoc>
        ///     <para>Gets the <see cref='Column' /> from the collection with the specified name.</para>
        /// </devdoc>
        public Column this[string name]
        {
            get
            {
                if (null == name) throw new ArgumentNullException("name");
                Column column;
                if (!_columnFromName.TryGetValue(name, out column) || column == null)
                {
                    // Case-Insensitive compares
                    var index = IndexOfCaseInsensitive(name);
                    if (0 <= index)
                        column = _list[index];
                    else if (-2 == index) throw new ApplicationException("Case sensitive name conflict");
                }

                return column;
            }
        }

        /// <devdoc>
        ///     <para>Gets or sets the locale information used to compare strings within the table.</para>
        ///     <para>Also used for locale sensitive, case, kana, width insensitive column name lookups</para>
        ///     <para>Also used for converting values to and from string</para>
        /// </devdoc>
        public CultureInfo Locale
        {
            get
            {
                // used for Comparing not Formatting/Parsing
                Debug.Assert(null != _culture, "null culture");
                Debug.Assert(_cultureUserSet || _culture.Equals(Locale), "Locale mismatch");
                return _culture;
            }
            set
            {
                var userSet = true;
                if (null == value)
                {
                    // reset Locale to inherit from DataSet
                    userSet = false;
                    value = Locale;
                }

                if (_culture != value && !_culture.Equals(value))
                {
                    var flag = false;
                    var exceptionThrown = false;
                    var oldLocale = _culture;
                    var oldUserSet = _cultureUserSet;
                    try
                    {
                        _cultureUserSet = true;
                        SetLocaleValue(value, true);
                    }
                    catch
                    {
                        exceptionThrown = true;
                        throw;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            // reset old locale if ValidationFailed or exception thrown
                            try
                            {
                                SetLocaleValue(oldLocale, true);
                            }
                            catch (Exception e)
                            {
                                // failed to reset all indexes for all constraints
                            }

                            _cultureUserSet = oldUserSet;
                            if (!exceptionThrown) throw new ApplicationException("Cannot change case locale");
                        }
                    }

                    SetLocaleValue(value, true);
                }

                _cultureUserSet = userSet;
            }
        }

        /// <devdoc>
        ///     <para>
        ///         Adds the specified <see cref='Column' />
        ///         to the columns collection.
        ///     </para>
        /// </devdoc>
        public void Add(Column column)
        {
            AddAt(-1, column);
        }

        internal void AddAt(int index, Column column)
        {
            BaseAdd(column);
            if (index != -1)
                ArrayAdd(index, column);
            else
                ArrayAdd(column);
        }

        /// <devdoc>
        ///     <para>
        ///         Creates and adds a <see cref='Column' />
        ///         with the
        ///         specified name and type to the columns collection.
        ///     </para>
        /// </devdoc>
        public Column Add(string columnName, Type type)
        {
            var column = new Column { Name = columnName, Type = type };
            Add(column);
            return column;
        }

        /// <devdoc>
        ///     <para>
        ///         Creates and adds a <see cref='Column' />
        ///         with the specified name to the columns collection.
        ///     </para>
        /// </devdoc>
        public Column Add(string columnName)
        {
            var column = new Column { Name = columnName };
            Add(column);
            return column;
        }

        /// <devdoc>
        ///     <para>Creates and adds a <see cref='Column' /> to a columns collection.</para>
        /// </devdoc>
        public Column Add()
        {
            var column = new Column();
            Add(column);
            return column;
        }

        /// <devdoc>
        ///     Adds the column to the columns array.
        /// </devdoc>
        private void ArrayAdd(Column column)
        {
            _list.Add(column);
        }

        private void ArrayAdd(int index, Column column)
        {
            _list.Insert(index, column);
        }

        /// <devdoc>
        ///     Creates a new default name.
        /// </devdoc>
        internal string AssignName()
        {
            var newName = MakeName(_defaultNameIndex++);

            while (_columnFromName.ContainsKey(newName)) newName = MakeName(_defaultNameIndex++);

            return newName;
        }

        /// <devdoc>
        ///     Makes a default name with the given index.  e.g. Column1, Column2, ... Columni
        /// </devdoc>
        private string MakeName(int index)
        {
            if (1 == index) return "Column1";
            return "Column" + index.ToString(CultureInfo.InvariantCulture);
        }

        /// <devdoc>
        ///     Registers this name as being used in the collection.  Will throw an ArgumentException
        ///     if the name is already being used.  Called by Add, All property, and Column.ColumnName property.
        ///     if the name is equivalent to the next default name to hand out, we increment our defaultNameIndex.
        ///     NOTE: To add a child table, pass column as null
        /// </devdoc>
        internal void RegisterColumnName(string name, Column column)
        {
            Debug.Assert(name != null);

            try
            {
                _columnFromName.Add(name, column);
            }
            catch (ArgumentException)
            {
                // Argument exception means that there is already an existing key
                throw new DuplicateNameException("Cannot add a duplicated column");
            }
        }

        internal bool CanRegisterName(string name)
        {
            Debug.Assert(name != null, "Must specify a name");
            return !_columnFromName.ContainsKey(name);
        }

        /// <devdoc>
        ///     Does verification on the column and it's name, and points the column at the dataSet that owns this collection.
        ///     An ArgumentNullException is thrown if this column is null.  An ArgumentException is thrown if this column
        ///     already belongs to this collection, belongs to another collection.
        ///     A DuplicateNameException is thrown if this collection already has a column with the same
        ///     name (case insensitive).
        /// </devdoc>
        private void BaseAdd(Column column)
        {
            if (column == null) throw new ArgumentNullException("column", "Column is null");

            if (column.Name.Length == 0) column.Name = AssignName();
            RegisterColumnName(column.Name, column);
        }

        /// <devdoc>
        ///     <para>Checks whether the collection contains a column with the specified name.</para>
        /// </devdoc>
        public bool Contains(string name)
        {
            Column column;
            if (_columnFromName.TryGetValue(name, out column) && column != null) return true;

            return IndexOfCaseInsensitive(name) >= 0;
        }

        internal bool Contains(string name, bool caseSensitive)
        {
            Column column;
            if (_columnFromName.TryGetValue(name, out column) && column != null) return true;

            if (caseSensitive)
                // above check did case sensitive check
                return false;
            return IndexOfCaseInsensitive(name) >= 0;
        }

        /// <devdoc>
        ///     <para>
        ///         Returns the index of a specified <see cref='Column' />.
        ///     </para>
        /// </devdoc>
        public int IndexOf(Column column)
        {
            var columnCount = _list.Count;
            for (var i = 0; i < columnCount; ++i)
                if (column == (Column) _list[i])
                    return i;
            return -1;
        }

        /// <devdoc>
        ///     <para>
        ///         Returns the index of
        ///         a column specified by name.
        ///     </para>
        /// </devdoc>
        public int IndexOf(string columnName)
        {
            if (null != columnName && 0 < columnName.Length)
            {
                var count = _list.Count;
                Column column;
                if (_columnFromName.TryGetValue(columnName, out column) && column != null)
                {
                    for (var j = 0; j < count; j++)
                        if (column == _list[j])
                            return j;
                }
                else
                {
                    var res = IndexOfCaseInsensitive(columnName);
                    return res < 0 ? -1 : res;
                }
            }

            return -1;
        }

        internal int IndexOfCaseInsensitive(string name)
        {
            var hashcode = GetSpecialHashCode(name);
            var cachedI = -1;
            Column column = null;
            for (var i = 0; i < _list.Count; i++)
            {
                column = (Column) _list[i];
                if ((hashcode == 0 || column._hashCode == 0 || column._hashCode == hashcode) &&
                    NamesEqual(column.Name, name, false) != 0)
                {
                    if (cachedI == -1)
                        cachedI = i;
                    else
                        return -2;
                }
            }

            return cachedI;
        }

        // Return value: 
        // > 0 (1)  : CaseSensitve equal      
        // < 0 (-1) : Case-Insensitive Equal
        // = 0      : Not Equal
        private int NamesEqual(string s1, string s2, bool fCaseSensitive)
        {
            if (fCaseSensitive) return string.Compare(s1, s2, false, Locale) == 0 ? 1 : 0;

            // Case, kana and width -Insensitive compare
            if (Locale.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth) == 0)
                return string.Compare(s1, s2, false, Locale) == 0 ? 1 : -1;
            return 0;
        }


        // We need a HashCodeProvider for Case, Kana and Width insensitive
        private int GetSpecialHashCode(string name)
        {
            int i;
            for (i = 0; i < name.Length && 0x3000 > name[i]; ++i) ;

            if (name.Length == i)
            {
                return _hashCodeProvider.GetHashCode(name);
            }

            return 0;
        }

        internal bool SetLocaleValue(CultureInfo culture, bool userSet)
        {
            Debug.Assert(null != culture, "SetLocaleValue: no locale");
            if (userSet || !_cultureUserSet && !_culture.Equals(culture))
            {
                _culture = culture;
                _hashCodeProvider = null;

                foreach (Column column in _list) column._hashCode = GetSpecialHashCode(column.Name);
                return true;
            }

            return false;
        }

        #region Implementation of IEnumerable

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void CopyTo(Column[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (array.Length - index < _list.Count)
            {
                throw new ArgumentException("Invalid offset length", "index");
            }
            for (int i = 0; i < _list.Count; ++i)
            {
                array[index + i] = (Column)_list[i];
            }
        }

        public bool Remove(Column item)
        {
            return _list.Remove(item);
        }

        /// <devdoc>
        ///     <para>
        ///         Clears the collection of any columns.
        ///     </para>
        /// </devdoc>
        public void Clear()
        {
            _list.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public bool Contains(Column item)
        {
            if (item == null)
            {
                return false;
            }
            return Contains(item.Name);
        }

        #endregion
    }
}