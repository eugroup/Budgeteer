#include "opencv2/opencv.hpp"
#include "iostream"
 
using namespace cv;
using namespace std;
 
int main(int argc, char** argv)
{
 
       Mat image;
       image = imread(argv[1], CV_LOAD_IMAGE_COLOR); 
 
       if(! image.data )                            
       {
              cout <<  "Could not open or find the image" << std::endl ;
              return -1;
       }

 
       // convert RGB image to gray
       cvtColor(image, image, CV_BGR2GRAY);

	blur(image, image, Size(5,5), Point(-1,-1), BORDER_DEFAULT);

       threshold(image, image, 128, 255, THRESH_BINARY | THRESH_OTSU);
	// adaptiveThreshold(gray, wb, 255,ADAPTIVE_THRESH_GAUSSIAN_C, THRESH_BINARY,11,2);
 
       imwrite(argv[2], image);
 
       waitKey(0);                                         
       return 0;
}
