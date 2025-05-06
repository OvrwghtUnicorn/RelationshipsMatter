using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using MelonLoader;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

namespace RelationshipsMatter {
    public static class Utility {
        public static void MergeSortUI(List<LocationData> items, Comparison<LocationData> comparison) {
            int n = items.Count;

            for (int currSize = 1; currSize <= n - 1; currSize = 2 * currSize) {
                for (int leftStart = 0; leftStart < n - 1; leftStart += 2 * currSize) {
                    int mid = Mathf.Min(leftStart + currSize - 1, n - 1);
                    int rightEnd = Mathf.Min(leftStart + 2 * currSize - 1, n - 1);

                    MergeUI(items, leftStart, mid, rightEnd, comparison);
                }
            }
        }

        public static void MergeUI(List<LocationData> items, int left, int mid, int right, Comparison<LocationData> comparison) {
            int n1 = mid - left + 1;
            int n2 = right - mid;

            List<LocationData> leftList = new List<LocationData>();
            List<LocationData> rightList = new List<LocationData>();

            for (int i = 0; i < n1; i++)
                leftList.Add(items[left + i]);
            for (int j = 0; j < n2; j++)
                rightList.Add(items[mid + 1 + j]);

            int iL = 0, iR = 0, k = left;

            while (iL < n1 && iR < n2) {
                if (comparison(leftList[iL], rightList[iR]) <= 0) {
                    items[k] = leftList[iL];
                    items[k].ButtonGo.transform.SetSiblingIndex(k);
                    iL++;
                } else {
                    items[k] = rightList[iR];
                    items[k].ButtonGo.transform.SetSiblingIndex(k);
                    iR++;
                }
                k++;
            }

            while (iL < n1) {
                items[k] = leftList[iL];
                items[k].ButtonGo.transform.SetSiblingIndex(k);
                iL++;
                k++;
            }

            while (iR < n2) {
                items[k] = rightList[iR];
                items[k].ButtonGo.transform.SetSiblingIndex(k);
                iR++;
                k++;
            }
        }
    }
}
