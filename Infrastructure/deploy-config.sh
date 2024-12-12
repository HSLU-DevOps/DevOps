#!/usr/bin/env bash
# This script creates the Docker Configs used to deploy the infrastructure.

# Exit the script if an error occurs
set -euo pipefail

# Constants
PREFIX_ENV="INFRA_CONFIGS"
STACK_NAME="infra"

# Sets the Docker Config that should be used for the deployment
deploy_docker_config() {
  echo "Deploying with Docker Config: ${config_deploy}"
  if [[ -v "${GITHUB_ENV:-}" ]]; then
    echo "${PREFIX_ENV}_${name^^}=${config_deploy}" >>"$GITHUB_ENV"
  fi
}

# Main
for config_file in config/*.yml; do
  echo "############################################################"
  echo "Processing config file: ${config_file}"
  name=$(basename "${config_file}" .yml)

  # Define Docker Config content, name and version
  config_base64=$(base64 "${config_file}" --wrap=0)
  config_name="${STACK_NAME}_${name}"
  config_version="1"

  # Get existing Docker Configs
  mapfile -t docker_configs < <(docker config ls --format "{{ .Name }}" --filter name="${config_name}")

  # Create a Docker Config if it doesn't exist yet
  if [[ -z "${docker_configs[*]}" ]]; then
    echo "Docker Config not found: ${config_name}"
    config_deploy="${config_name}_v${config_version}"
    docker config create "${config_deploy}" "${config_file}" >/dev/null
    echo "Docker Config created: ${config_deploy}"
    deploy_docker_config
    continue
  fi

  # Extract latest Docker Config content, name and version
  config_name_latest="${docker_configs[-1]}"
  config_version="${config_name_latest#"${config_name}"_v}"
  config_base64_latest="$(docker config inspect "${config_name_latest}" --format "{{ json .Spec.Data }}" | tr -d '"')"
  echo "Latest Docker Config name: ${config_name_latest}"
  echo "Latest Docker Config version: ${config_version}"

  # Create a new version of the Docker Config if the file changed
  if [[ "${config_base64}" != "${config_base64_latest}" ]]; then
    echo "Configuration file changed: ${config_file}"
    config_version=$((config_version + 1))
    config_deploy="${config_name}_v${config_version}"
    docker config create "${config_deploy}" "${config_file}" >/dev/null
    docker_configs+=("${config_deploy}")
    echo "Docker Config created: ${config_deploy}"
  else
    echo "Configuration file unchanged: ${config_file}"
    config_deploy="${config_name}_v${config_version}"
  fi

  # Remove old Docker Config versions, keeping the last three versions
  if [[ "${#docker_configs[@]}" -gt 3 ]]; then
    delete_configs=("${docker_configs[@]:0:$((${#docker_configs[@]} - 3))}")
    for config_delete in "${delete_configs[@]}"; do
      docker config rm "${config_delete}" >/dev/null
      echo "Docker Config deleted: ${config_delete}"
    done
  fi

  deploy_docker_config
done
