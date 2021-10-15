using System.Collections.Generic;
using System.Linq;

namespace KeyValList
{
    public class KeyValList
    {
        private List< KeyValuePair<string,string > > vars;
        public KeyValList()
        {
            vars = new List<KeyValuePair<string, string>>();
        }
        /// <summary>
        /// возвращает ключ хранящийся в позиции index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index] => vars[index].Key;

        /// <summary>
        /// организует доступ к паре ключ, значение по имени ключа
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get => (from pair in vars where pair.Key == key select pair.Value).FirstOrDefault();

            set
            {
                for (var i = 0; i < vars.Count; i++)
                {
                    if (vars[i].Key == key)
                    {
                        if( vars[i].Value != value )
                            vars[i] = new KeyValuePair<string,string>( key, value );
                        return;
                    }
                }
                vars.Add(new KeyValuePair<string, string>(key, value));
            }
        }
        /// <summary>
        /// Возвращает позицию ключа в списке
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int IndexOfKey(string key)
        {
            for (var i = 0; i < vars.Count; i++)
                if (vars[i].Key == key)
                    return i;
            return -1;
        }
        /// <summary>
        /// Удаляет пару ключ, значение из позиции idx
        /// </summary>
        /// <param name="idx"></param>
        public void Remove(int idx)
        {
            vars.RemoveAt(idx);
        }

        /// <summary>
        /// Вставляет пару ключ,значение в позицию idx. Внимание! Не проверяет наличие key в списке
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Insert(int idx, string key, string value)
        {
            vars.Insert(idx, new KeyValuePair<string, string>(key, value));     
        }

        /// <summary>
        /// Добавляет пару ключ,значение в конец списка. Внимание! Не проверяет наличие key в списке
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value)
        {
            vars.Add(new KeyValuePair<string, string>(key, value));
        }
        /// <summary>
        /// возвращает количество сохраненных значение
        /// </summary>
        public int Count => vars.Count;
    }
}
