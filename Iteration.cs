using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Jay.IEnumerators
{
    public class Iteration
    {
        private int _StartX;
        private int _StartY;
        private int _BoundX;
        private int _BoundY;

        public Iteration(int _StartX, int _StartY, int _BoundX, int _BoundY)
        {
            this._StartX = _StartX % _BoundX;
            this._StartY = _StartY % _BoundY;
            this._BoundX = _BoundX;
            this._BoundY = _BoundY;
        }

        public IEnumerator<int> Linear()
        {
            while(true)
            {
                yield return _StartY * _BoundX + _StartX;
                _StartX++;
                if(_StartX >= _BoundX)
                {
                    _StartY++;
                    _StartX = 0;
                    if(_StartY >= _BoundY)
                    {
                        _StartY = 0;
                    }
                }
            }
        }

        public IEnumerator<int> Reverse()
        {
            while(true)
            {
                yield return _StartY * _BoundX + _StartX;
                _StartX--;
                if(_StartX < 0)
                {
                    _StartX = _BoundX - 1;
                    _StartY--;
                    if(_StartY < 0)
                    {
                        _StartY = _BoundY;
                    }
                }
            }
        }
    }
}