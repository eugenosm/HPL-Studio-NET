using System.Collections.Generic;
using System.Linq;

namespace KeyValList
{
    public class KeyValList
    {
        public KeyValList()
        {
            List = new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> List { get; }

        /// <summary>
        /// возвращает ключ хранящийся в позиции index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index] => List[index].Key;

        /// <summary>
        /// организует доступ к паре ключ, значение по имени ключа
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get => (from pair in List where pair.Key == key select pair.Value).FirstOrDefault();

            set
            {
                for (var i = 0; i < List.Count; i++)
                {
                    if (List[i].Key == key)
                    {
                        if( List[i].Value != value )
                            List[i] = new KeyValuePair<string,string>( key, value );
                        return;
                    }
                }
                List.Add(new KeyValuePair<string, string>(key, value));
            }
        }
        /// <summary>
        /// Возвращает позицию ключа в списке
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int IndexOfKey(string key)
        {
            for (var i = 0; i < List.Count; i++)
                if (List[i].Key == key)
                    return i;
            return -1;
        }
        /// <summary>
        /// Удаляет пару ключ, значение из позиции idx
        /// </summary>
        /// <param name="idx"></param>
        public void Remove(int idx)
        {
            List.RemoveAt(idx);
        }

        /// <summary>
        /// Вставляет пару ключ,значение в позицию idx. Внимание! Не проверяет наличие key в списке
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Insert(int idx, string key, string value)
        {
            List.Insert(idx, new KeyValuePair<string, string>(key, value));     
        }

        /// <summary>
        /// Добавляет пару ключ,значение в конец списка. Внимание! Не проверяет наличие key в списке
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value)
        {
            List.Add(new KeyValuePair<string, string>(key, value));
        }
        /// <summary>
        /// возвращает количество сохраненных значение
        /// </summary>
        public int Count => List.Count;
    }
}
