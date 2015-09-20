<?php
$notAllowWords = array("fuck","shit");
$wordFilter = true;

if (isset($_POST['danmaku'])) {
	$msg = $_POST['danmaku'];
	//word filter
	if ($wordFilter) {
		foreach ($notAllowWords as $item) {
			$msg = str_replace($item, "[Not allow]", $msg);
		}
	}
	//base filter
	$msg = str_replace("\\", "\\\\", $msg);
	$msg = trim($msg);
	if ($msg == "") {
		exit();
	}
	//start send
	$addr = '127.0.0.1';
	$port = '8585';
	//Create Socket
	$sock = socket_create(AF_INET, SOCK_DGRAM, SOL_UDP);
	//Send to OhMyDanmaku Server
	socket_sendto($sock, $msg, strlen($msg), 0, $addr, $port);
	//CLose Socket
	socket_close($sock);
	exit();
}

?>
