#!/bin/bash

LOGS_DIR="$(pwd)/logs"

# check if script was called with correct arguments
if (( $# < 2 )); then
  echo "Usage: $0 num_threads additional_parameters_for_dotnet"
  exit 1
fi

cd Chess-Challenge

# set number of threads and shift to remove it from parameters
num_threads=$1
shift

# get number of lines in the file
num_lines=$(wc -l < resources/Fens.txt)

# calculate range per thread
range_per_thread=$((num_lines / num_threads))

# create a directory to store the logs
mkdir -p $LOGS_DIR

# spawn threads
for ((i=0; i<num_threads; i++)); do
  start=$((i * range_per_thread))
  end=$(((i+1) * range_per_thread - 1))

  # handle last thread differently if num_lines is not a multiple of num_threads
  if (( i == num_threads - 1 )); then
    end=$num_lines
  fi

  echo "=== benchmark.sh starting thread $i: $start - $end ===" | tee "$LOGS_DIR/thread_$i.log"
  # run the dotnet program and tee output to a log file
  FENS_START=$start FENS_END=$end dotnet run $@ > >(tee "$LOGS_DIR/thread_$i.log") &
  pid=$!

  # spawn a separate sub-process to monitor the log file and kill the dotnet process when it finishes
  (while true; do
    if grep -q 'Match finished:' "$LOGS_DIR/thread_$i.log"; then
      echo "=== benchmark.sh killing thread $i ==="
      kill $pid
      break
    fi
    sleep 1
  done) &
  pids[$i]=$pid

  # wait until the dotnet program has started
  while true; do
    if grep -q 'Launching Chess-Challenge' "$LOGS_DIR/thread_$i.log"; then
      break
    fi
    sleep 1
  done
done

# initialize total wins, draws and losses
total_wins=0
total_draws=0
total_losses=0

# wait for each dotnet program to finish and gather wins, draws, losses
for ((i=0; i<num_threads; i++)); do
  # wait for the dotnet program to finish
  wait ${pids[$i]}

  # parse the log file for wins, draws and losses
  wins=$(grep -oP 'Match finished: \+\K\d+' "$LOGS_DIR/thread_$i.log")
  draws=$(grep -oP 'Match finished: .+\=\K\d+' "$LOGS_DIR/thread_$i.log")
  losses=$(grep -oP 'Match finished: .+ -\K\d+' "$LOGS_DIR/thread_$i.log")

  # increment total wins, draws and losses
  ((total_wins += wins))
  ((total_draws += draws))
  ((total_losses += losses))
done

# print the total number of wins, draws and losses
echo "Benchmark finished: +$total_wins =$total_draws -$total_losses"

