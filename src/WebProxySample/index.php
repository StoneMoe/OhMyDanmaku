<?php

if (isset($_POST['danmaku'])) {
	$addr = '127.0.0.1';
	$port = '8585';
	//Create socket
	$sock = socket_create(AF_INET, SOCK_DGRAM, SOL_UDP);
	socket_sendto($sock, $_POST['danmaku'], strlen($_POST['danmaku']), 0, $addr, $port);
	socket_close($sock);
	echo 
<<<EOFoo
<html>
<header>
	<title>OhMyDanmaku Web DEMO</title>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width, inital-scale=1" /><!--For Mobile Browsers Display-->
	<style>
		width: 100%;
		height: 100%;
	</style>
</header>

<body>
	<center>
		<br>
		<br>
		<h3>发送成功!</h3> 
		<br>
		<br> 
		<a href="index.php">返回</a>
	</center>
</body>

</html>
EOFoo;
exit();
}

echo 
<<<EOFoo
<html>
<header>
	<title>OhMyDanmaku Web DEMO</title>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width, inital-scale=1" /><!--For Mobile Browsers Display-->
	<style>
		width: 100%;
		height: 100%;
	</style>
</header>
<body>
	<center>
		<br>
		<br>
		<h2>( ·ω·)/~</h2>
		<form action="index.php" method="POST">
			<input type="text" name="danmaku" placeholder="在此输入弹幕"><br><br>
			<input type="submit" value="发送!">
		</form>
	</center>
</body>
</html>
EOFoo;
?>
