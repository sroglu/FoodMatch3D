using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Game.Editor.Utilities
{
    public static class EditorCoroutine
    {
        public static IEnumerator Start(IEnumerator routine)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(routine);

            void Update()
            {
                if (stack.Count == 0)
                {
                    EditorApplication.update -= Update;
                    return;
                }

                var current = stack.Peek();
                if (current.MoveNext())
                {
                    if (current.Current is IEnumerator nested)
                    {
                        stack.Push(nested);
                    }
                }
                else
                {
                    stack.Pop();
                }
            }

            EditorApplication.update += Update;
            return routine;
        }

        public static void StopAllCoroutines()
        {
            EditorApplication.update = null;
        }
        
        public static IEnumerator WaitForSeconds(float seconds)
        {
            float startTime = (float)EditorApplication.timeSinceStartup;
            while ((float)EditorApplication.timeSinceStartup - startTime < seconds)
            {
                yield return null;
            }
        }
        
        public static IEnumerator WaitUntil(Func<bool> predicate, Action action)
        {
            while (!predicate())
            {
                action?.Invoke();
                yield return null;
            }
        }
        
    }
}