namespace FPSCounter
{
    class SmartFramerate
    {
        private double _currentFrametimes;
        private double _weight;
        private int _numerator;

        public double Framerate => _numerator / _currentFrametimes;

        public SmartFramerate(int oldFrameWeight)
        {
            _numerator = oldFrameWeight;
            _weight = (double)oldFrameWeight / ((double)oldFrameWeight - 1d);
        }

        public void Update(double timeSinceLastFrame)
        {
            _currentFrametimes = _currentFrametimes / _weight;
            _currentFrametimes += timeSinceLastFrame;
        }
    }
}
