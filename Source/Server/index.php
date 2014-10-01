<?php

header("Content-Type: text/plain");

$timestamp = time();

if(isset($_GET["url"]))
	$img_data = file_get_contents(urldecode($_GET["url"]));
else
	$img_data = file_get_contents("php://input");

$fid = fopen("$timestamp.jpg","w+");
fwrite($fid, $img_data);
fclose($fid);	

exec("/bin/bash gen_txt $timestamp.jpg", $output);
echo file_get_contents("$timestamp.jpg.txt");

unlink("$timestamp.jpg");
unlink("output_$timestamp.jpg");
unlink("$timestamp.jpg.txt");

?>