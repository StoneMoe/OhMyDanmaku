<?php

if (isset($_POST['danmaku'])) {
	$addr = '127.0.0.1';
	$port = '8585';

	//Create Socket
	$sock = socket_create(AF_INET, SOCK_DGRAM, SOL_UDP);

	//Send to OhMyDanmaku Server
	socket_sendto($sock, $_POST['danmaku'], strlen($_POST['danmaku']), 0, $addr, $port);

	//CLose Socket
	socket_close($sock);

	exit();
}

?>
