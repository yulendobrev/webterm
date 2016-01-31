#include <stdio.h>
#include <unistd.h>
#include <termios.h>
#include <pty.h>
#include <poll.h>
#include <signal.h>
#include <sys/wait.h>

void print_usage()
{
	fprintf(stderr, "Usage: <command> <arguments ...>\n");
}

int transfer_data_chunk(int from, int to)
{
	char buffer[1024];
	int bytes_read, bytes_written;

	bytes_read = read(from, buffer, 1024);
	bytes_written = 0;
	do {
		int write_result = write(to, buffer + bytes_written, bytes_read - bytes_written);
		if (write_result <= 0) {
			break;
		}
		bytes_written += write_result;
	} while (bytes_written < bytes_read);

	return bytes_read - bytes_written;
}

void redirect_stdio_to_child(int masterFd, pid_t child)
{
	int ready;
	int transfer_success;
	struct pollfd fds[2];
	fds[0].fd = masterFd;
	fds[0].events = POLLIN;
	fds[1].fd = 0;
	fds[1].events = POLLIN;

	while ((ready = poll(fds, 2, -1)) > 0) {
		if (fds[0].revents & POLLIN) {
			if (transfer_data_chunk(masterFd, 1) != 0) {
				break;
			}
		} else if (fds[0].revents & (POLLERR | POLLHUP | POLLNVAL)) {
			break;
		}

		if (fds[1].revents & POLLIN) {
			if (transfer_data_chunk(0, masterFd) != 0) {
				break;
			}
		} else if (fds[1].revents & (POLLERR | POLLHUP | POLLNVAL)) {
			break;
		}
	}
}

tcflag_t disable_local_echo()
{
	struct termios term;
	tcgetattr(0, &term);

	tcflag_t lflag = term.c_lflag;
	term.c_lflag &= ~ICANON & ~ECHO;

	tcsetattr(0, TCSANOW, &term);

	return lflag;
}

void restore_local_echo(tcflag_t lflag)
{
	struct termios term;
	tcgetattr(0, &term);

	term.c_lflag = lflag;

	tcsetattr(0, TCSANOW, &term);
}

int main(int argc, char *argv[])
{
	struct termios term;
	term.c_iflag = BRKINT | ICRNL | IXON | IXANY | IMAXBEL;
	term.c_oflag = OPOST | ONLCR | NL0 | CR0 | TAB0 | BS0 | VT0 | FF0;
	term.c_cflag = CS8 | CREAD | HUPCL;
	term.c_lflag = ISIG | ICANON | IEXTEN | ECHO | ECHOE | ECHOK | ECHOCTL | ECHOKE;

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
        int status;
		tcflag_t original_lflag = disable_local_echo();
		redirect_stdio_to_child(masterFd, child);
		restore_local_echo(original_lflag);

		kill(child, SIGKILL);
		wait(&status);

		return status;
	}

	return 0;
}
