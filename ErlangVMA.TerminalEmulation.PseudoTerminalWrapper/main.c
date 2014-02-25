#include <stdio.h>
#include <unistd.h>
#include <termios.h>
#include <pty.h>
#include <poll.h>

void print_usage()
{
	fprintf(stderr, "Usage: <command> <arguments ...>\n");
}

void transfer_data_chunk(int from, int to)
{
	char buffer[1024];
	int bytes_read, bytes_written;

	bytes_read = read(from, buffer, 1024);
	bytes_written = 0;
	do {
		bytes_written += write(to, buffer + bytes_written, bytes_read - bytes_written);
	} while (bytes_written > 0 && bytes_written < bytes_read);
}

void redirect_stdio_to_child(int masterFd, pid_t child)
{
	int ready;
	struct pollfd fds[2];
	fds[0].fd = masterFd;
	fds[0].events = POLLIN;
	fds[1].fd = 0;
	fds[1].events = POLLIN;

	while ((ready = poll(fds, 2, -1)) > 0) {
		if (fds[0].revents & POLLIN) {
			transfer_data_chunk(masterFd, 1);
		} else if (fds[1].revents & POLLIN) {
			transfer_data_chunk(0, masterFd);
		}
	}
}

void disable_local_echo()
{
	struct termios term;
	tcgetattr(0, &term);

	term.c_lflag &= ~ICANON & ~ECHO;

	tcsetattr(0, TCSANOW, &term);
}

int main(int argc, char *argv[])
{
	struct termios term;
	term.c_iflag = 0;
	term.c_oflag = 0,
	term.c_cflag = 0;
	term.c_lflag = ISIG | ICANON | ECHO | ECHOE | ECHOK;

	struct winsize win;
	win.ws_row = 24;
	win.ws_col = 80;

	int masterFd;

	if (argc < 2) {
		print_usage();
		return 1;
	}

	pid_t child = forkpty(&masterFd, 0, &term, &win);
	if (child == -1) {
		fprintf(stderr, "Could not fork process with pseudo terminal set");
		return 2;
	}

	if (child == 0) {
		execv(argv[1], argv + 1);
	} else {
		disable_local_echo();
		redirect_stdio_to_child(masterFd, child);
	}

	return 0;
}
