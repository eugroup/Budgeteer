<?php
// default values
$output_type = "txt";
$lang = "eng";

// if get parameters are set default values are overwritten
if(isset($_GET["output"])) $output_type = $_GET["output"];
if(isset($_GET["lang"])) $lang = $_GET["lang"];

// generate a timestamp for the process below
$timestamp = time();

// determine the input type where the source image come from ( get / post )
if(isset($_GET["url"]))
	$img_data = file_get_contents(urldecode($_GET["url"]));
else
	$img_data = file_get_contents("php://input");

// write incoming raw data into file
$fid = fopen("$timestamp.jpg","w+");
fwrite($fid, $img_data);
fclose($fid);	

// determine current directory 
$PWD = realpath(dirname(__FILE__));


// pre-processing commands
// greyscaling + rotating etc.
exec("./textcleaner -g -e normalize -f 25 -o 10 -u -s 1 -T -p 10 $PWD/$timestamp.jpg pre_$timestamp.jpg");

// greyscaled image to black / white image
exec("./2colorthresh pre_$timestamp.jpg output_$timestamp.jpg");

// ocr command
exec("tesseract $PWD/output_$timestamp.jpg $timestamp.jpg -l $lang");

// response
switch($output_type){
	case "txt":
		header("Content-Type: text/plain");
		echo file_get_contents("$timestamp.jpg.txt");
		break;
	case "img":
		header("Content-Type: image/jpeg");
		echo file_get_contents("output_$timestamp.jpg");
		break;
}

// delete generated files aren't used anymore
unlink("$timestamp.jpg");
unlink("output_$timestamp.jpg");
unlink("pre_$timestamp.jpg");
unlink("$timestamp.jpg.txt");

?>