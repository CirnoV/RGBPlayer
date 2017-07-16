using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGBPlayer.Core
{
	/// <summary>
	/// 문자열 키를 가지는 마스터 테이블 정의
	/// </summary>
	/// <typeparam name="TValue">마스터 데이터 형식</typeparam>
	public class MasterTable<TValue> : IReadOnlyDictionary<string, TValue> where TValue : class, IIdentifiable
	{
		private readonly IDictionary<string, TValue> dictionary;

		/// <summary>
		/// Key를 통한 요소 접근, 없으면 null
		/// </summary>
		public TValue this[string key] => this.dictionary.ContainsKey(key) ? this.dictionary[key] : null;

		public MasterTable() : this(new List<TValue>()) { }
		public MasterTable(IEnumerable<TValue> source)
		{
			this.dictionary = source.ToDictionary(x => x.Key);
		}

		#region IReadOnlyDictionary<TK, TV> members
		public IEnumerable<string> Keys => this.dictionary.Keys;
		public IEnumerable<TValue> Values => this.dictionary.Values;
		public int Count => this.dictionary.Count;

		public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
		{
			return this.dictionary.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public bool ContainsKey(string key)
		{
			return this.dictionary.ContainsKey(key);
		}
		public bool TryGetValue(string key, out TValue value)
		{
			return this.dictionary.TryGetValue(key, out value);
		}
		#endregion
	}
}
