using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Jay.IEnumerators
{
    public class Iteration2D
    {
        private int _StartX;
        private int _StartY;
        private int _BoundX;
        private int _BoundY;

        public Iteration2D(int _StartX, int _StartY, int _BoundX, int _BoundY)
        {
            this._StartX = _StartX % _BoundX;
            this._StartY = _StartY % _BoundY;
            this._BoundX = _BoundX;
            this._BoundY = _BoundY;
        }

        public int XPos { get => _StartX; set=> _StartX = value % _BoundX; }
        public int YPos { get => _StartY; set=> _StartY = value % _BoundY; }

        public IEnumerator<Point> Linear()
        {
            while(true)
            {
                yield return new Point(_StartX, _StartY);
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

        public IEnumerator<Point> Reverse()
        {
            while(true)
            {
                yield return new Point(_StartX, _StartY);
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

        public IEnumerator<Point> TTB_LTR() => Linear();
        public IEnumerator<Point> BTT_RTL() => Reverse();

        public IEnumerator<Point> BTT_LTR() 
        {
            while(true)
            {
                yield return new Point(_StartX, _StartY);
                _StartX++;
                if(_StartX >= _BoundX)
                {
                    _StartX = 0;
                    _StartY--;
                    if(_StartY < 0)
                    {
                        _StartY = _BoundY;
                    }
                }
            }
        }

        public IEnumerator<Point> TTB_RTL()
        {
            while(true)
            {
                yield return new Point(_StartX, _StartY);
                _StartX--;
                if(_StartX < 0)
                {
                    _StartX = _BoundX - 1;
                    _StartY++;
                    if(_StartY >= _BoundY)
                    {
                        _StartY = _BoundY;
                    }
                }
            }
        }

        public IEnumerator<Point> Random()
        {
            Random rng = new Random();
            while(true)
            {
                yield return new Point(_StartX, _StartY);
                _StartX = rng.Next(0, _BoundX - 1);
                _StartY = rng.Next(0, _BoundY - 1);
            }
        }

        public IEnumerator<Point> Snake(int direction)
        {
            if(!(direction == 0 || direction == 90 || direction == 180 || direction == 270))
            {
                throw new IEnumeratorException("Snake iteration direction should either be 0, 90, 180 or 270 degrees.", "Unacceptable argument");
            }

            bool vertPrimary = (direction == 90 || direction == 270);
            int sxDir = (direction == 0 || direction == 90) ? +1 : -1;
            int syDir = (direction == 0 || direction == 270) ? +1 : -1;
            int xDir = (vertPrimary) ? sxDir : sxDir * ((_StartY % 2 == 0) ? +1 : -1);
            int yDir = (vertPrimary) ? syDir * ((_StartX % 2 == 0) ? +1 : -1) : syDir;
            if(vertPrimary)
            {
                while(true)
                {
                    yield return new Point(_StartX, _StartY);
                    _StartY += yDir;
                    if(_StartY >= _BoundY || _StartY < 0)
                    {
                        yDir = (yDir == +1) ? -1 : +1;
                        _StartY = (_StartY >= _BoundY) ? _BoundY - 1 : 0;
                        _StartX += xDir;
                        if(_StartX < 0 || _StartX >= _BoundX)
                        {
                            _StartX = (_StartX < 0) ? _BoundX - 1 : 0;
                        }
                    }
                }
            }
            else
            {
                while(true)
                {
                    yield return new Point(_StartX, _StartY);
                    _StartX += xDir;
                    if(_StartX >= _BoundX || _StartX < 0)
                    {
                        xDir = (xDir == +1) ? -1 : +1;
                        _StartX = (_StartX >= _BoundX) ? _BoundX - 1 : 0;
                        _StartY += yDir;
                        if(_StartY < 0 || _StartY >= _BoundY)
                        {
                            _StartY = (_StartY < 0) ? _BoundY - 1 : 0;
                        }
                    }
                }
            }
        }
    }
}